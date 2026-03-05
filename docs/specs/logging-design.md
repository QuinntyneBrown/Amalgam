# Logging System Design — Amalgam

## Overview

A structured logging system that captures logs from the ASP.NET Core Web API (`Amalgam.Api`), the unified backend host (`Amalgam.Host`), and the Angular web application (`Amalgam.Web`), stores them in a queryable format, and exposes them through an API so they can be passed to an LLM to generate fixes.

## Goals

1. Capture structured logs from both backend (.NET) and frontend (Angular) applications
2. Store logs in a format that preserves context (timestamps, severity, correlation IDs, source, stack traces)
3. Expose logs through a REST API for retrieval and filtering
4. Provide an LLM integration endpoint that packages relevant logs into a prompt and returns a suggested fix

## Architecture

```
┌──────────────┐   ┌──────────────┐   ┌──────────────────┐
│ Amalgam.Web  │   │ Amalgam.Api  │   │  Amalgam.Host    │
│  (Angular)   │   │  (Web API)   │   │  (Unified Host)  │
└──────┬───────┘   └──────┬───────┘   └──────┬───────────┘
       │                  │                   │
       │ POST /api/logs   │ ILogger<T>        │ ILogger<T>
       │                  │                   │
       ▼                  ▼                   ▼
  ┌──────────────────────────────────────────────┐
  │           Log Ingestion Middleware           │
  │         + LogCollectorService (sink)         │
  └──────────────────┬───────────────────────────┘
                     │
                     ▼
  ┌──────────────────────────────────────────────┐
  │         In-Memory Ring Buffer Store          │
  │      (recent N entries, per-session)         │
  └──────────────────┬───────────────────────────┘
                     │
            ┌────────┴────────┐
            ▼                 ▼
   GET /api/logs        POST /api/logs/diagnose
   (query/filter)       (send to LLM for fix)
```

## Components

### 1. Log Entry Model

A unified log entry that both backend and frontend logs normalize into.

```csharp
public class LogEntry
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public LogLevel Level { get; set; }           // Trace, Debug, Info, Warning, Error, Critical
    public string Source { get; set; }             // "Api", "Host", "Web"
    public string Category { get; set; }           // logger category / Angular service name
    public string Message { get; set; }
    public string? Exception { get; set; }         // full exception string including stack trace
    public string? CorrelationId { get; set; }     // ties related logs across request lifecycle
    public Dictionary<string, string>? Properties { get; set; }  // structured data
}
```

### 2. LogCollectorService (In-Memory Store)

A singleton service that acts as both an `ILogger` provider/sink and a queryable store.

```csharp
public class LogCollectorService
{
    private readonly ConcurrentQueue<LogEntry> _entries;
    private readonly int _maxEntries;              // configurable, default 10,000

    public void Add(LogEntry entry);
    public IReadOnlyList<LogEntry> Query(LogQueryFilter filter);
    public void Clear();
}

public class LogQueryFilter
{
    public LogLevel? MinLevel { get; set; }
    public string? Source { get; set; }
    public string? Category { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public string? SearchText { get; set; }        // substring match on Message or Exception
    public string? CorrelationId { get; set; }
    public int? Limit { get; set; }                // max entries to return, default 200
}
```

**Why in-memory**: This is a developer tool, not a production telemetry pipeline. A bounded ring buffer keeps things simple, fast, and zero-dependency. No database or file system setup required.

### 3. LogCollectorProvider (ILoggerProvider)

A custom `ILoggerProvider` that plugs into the standard .NET logging pipeline and routes log entries into `LogCollectorService`.

```csharp
public class LogCollectorProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName);
}

public class LogCollectorLogger : ILogger
{
    // Writes to LogCollectorService.Add(...)
}
```

**Registration** in `Program.cs`:

```csharp
builder.Logging.AddProvider(new LogCollectorProvider(logCollectorService));
```

This means all existing `ILogger<T>` usage in controllers, services, and middleware automatically flows into the collector with zero code changes to existing classes.

### 4. Request Logging Middleware

Middleware that captures HTTP request/response details and attaches a correlation ID.

```csharp
public class RequestLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // Log request
        // Call next middleware
        // Log response (status code, duration)
        // On exception: log with full stack trace
    }
}
```

### 5. Frontend Log Ingestion Endpoint

The Angular app sends logs to the API via a POST endpoint. This captures client-side errors, Angular `ErrorHandler` output, and console-level logs.

**API Endpoint**: `POST /api/logs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    [HttpPost]
    public IActionResult Ingest([FromBody] FrontendLogEntry[] entries);

    [HttpGet]
    public IActionResult Query([FromQuery] LogQueryFilter filter);

    [HttpPost("diagnose")]
    public async Task<IActionResult> Diagnose([FromBody] DiagnoseRequest request);

    [HttpDelete]
    public IActionResult Clear();
}
```

**Angular side** — a `LogCollectorService` (Angular) that captures errors and sends them in batches:

```typescript
@Injectable({ providedIn: 'root' })
export class LogCollectorService {
  private buffer: LogEntry[] = [];
  private flushInterval = 5000; // ms

  constructor(private http: HttpClient) {
    setInterval(() => this.flush(), this.flushInterval);
  }

  log(level: LogLevel, category: string, message: string, error?: any): void {
    this.buffer.push({ timestamp: new Date().toISOString(), level, category, message, exception: error?.stack });
  }

  private flush(): void {
    if (this.buffer.length === 0) return;
    const batch = [...this.buffer];
    this.buffer = [];
    this.http.post('/api/logs', batch).subscribe();
  }
}
```

A custom Angular `ErrorHandler` routes uncaught errors into this service:

```typescript
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private logCollector: LogCollectorService) {}

  handleError(error: any): void {
    this.logCollector.log('Error', 'Angular', error.message, error);
  }
}
```

### 6. LLM Diagnose Endpoint

The core feature — package logs and send them to an LLM for analysis.

**Request**:

```csharp
public class DiagnoseRequest
{
    public LogQueryFilter? Filter { get; set; }    // which logs to include
    public string? AdditionalContext { get; set; }  // user-provided description of the problem
}
```

**Response**:

```csharp
public class DiagnoseResponse
{
    public string Diagnosis { get; set; }           // LLM's analysis of the issue
    public string? SuggestedFix { get; set; }       // code or config change suggestion
    public int LogsAnalyzed { get; set; }           // how many log entries were sent
}
```

**Flow**:

1. Query `LogCollectorService` using the filter (defaults to last 200 error/warning entries)
2. Build a prompt containing:
   - System message describing the Amalgam architecture
   - The log entries formatted as structured text
   - The user's additional context (if any)
3. Call the configured LLM API (e.g., Anthropic Claude API)
4. Return the structured response

**Prompt construction**:

```
You are analyzing logs from a .NET + Angular application called Amalgam.
The application has these components:
- Amalgam.Api: ASP.NET Core Web API for configuration management
- Amalgam.Host: Unified backend host that composes microservices
- Amalgam.Web: Angular frontend with Angular Material

Here are the recent log entries:
---
[Timestamp] [Level] [Source/Category] Message
Exception (if any)
---

{logs}

User context: {additionalContext}

Analyze these logs. Identify the root cause of any errors or warnings.
Provide a specific, actionable fix including code changes if applicable.
```

### 7. Configuration

In `appsettings.json`:

```json
{
  "Logging": {
    "Collector": {
      "MaxEntries": 10000,
      "MinLevel": "Information"
    }
  },
  "Diagnostics": {
    "LlmProvider": "Anthropic",
    "ApiKey": "${ANTHROPIC_API_KEY}",
    "Model": "claude-sonnet-4-20250514",
    "MaxLogsPerRequest": 200
  }
}
```

## Integration Points

### Amalgam.Api — Changes

- `Program.cs`: Register `LogCollectorService`, `LogCollectorProvider`, `RequestLoggingMiddleware`
- Add `LogsController`
- No changes to existing controllers or services

### Amalgam.Host — Changes

- `Program.cs`: Register `LogCollectorService`, `LogCollectorProvider`, `RequestLoggingMiddleware`
- Existing `ErrorDiagnostics` and `RoutePrefixMiddleware` remain unchanged

### Amalgam.Web — Changes

- Add `LogCollectorService` to the `api` library
- Add `GlobalErrorHandler` to the `app` module
- Add a **Log Viewer** component to the `domain` library for the UI (optional, future)

## Project Structure

```
src/
├── Amalgam.Api/
│   └── Controllers/
│       └── LogsController.cs           # new
├── Amalgam.Core/
│   └── Logging/
│       ├── LogEntry.cs                 # new
│       ├── LogQueryFilter.cs           # new
│       ├── LogCollectorService.cs      # new
│       ├── LogCollectorProvider.cs     # new
│       ├── RequestLoggingMiddleware.cs # new
│       ├── DiagnoseRequest.cs          # new
│       ├── DiagnoseResponse.cs         # new
│       └── LlmDiagnosticService.cs    # new
├── Amalgam.Web/
│   └── projects/
│       └── api/
│           └── src/lib/
│               ├── log-collector.service.ts    # new
│               └── global-error-handler.ts     # new
```

## Security Considerations

- The LLM API key is read from environment variables, never checked into source
- The diagnose endpoint is intended for local development use only
- Log entries may contain sensitive data; the ring buffer is ephemeral and cleared on restart
- Frontend log ingestion endpoint should be rate-limited to prevent abuse

## Future Extensions

- **Persistent storage**: Optionally write logs to SQLite for post-mortem analysis
- **Log Viewer UI**: An Angular component in the web app that displays logs in real time with filtering
- **WebSocket streaming**: Push new log entries to the frontend in real time
- **Auto-diagnose**: Automatically trigger LLM analysis when error rate exceeds a threshold
