# Design: Multi-Platform Git Support

## 1. Problem Statement

Amalgam currently assumes all repositories exist as local filesystem directories. There is no ability to clone, sync, or interact with remote git repositories. Teams using different git hosting platforms (GitHub, GitLab, Bitbucket, self-hosted GitLab at custom domains like `git.scm.*`) must manually clone and manage repositories outside of Amalgam before configuring them.

This design introduces a platform-agnostic git abstraction layer so Amalgam can clone, pull, and manage repositories from any git hosting provider.

## 2. Goals

- Support GitHub, GitLab (cloud and self-hosted), Bitbucket (cloud and server), and any git-compatible host
- Clone remote repositories automatically when they are not present locally
- Pull/sync repositories on demand
- Authenticate via tokens, SSH keys, or credential helpers — never store secrets in `amalgam.yml`
- Remain backward-compatible: existing configs with only `path` continue to work unchanged
- Keep the abstraction thin — Amalgam is not a git client; it delegates to `git` CLI

## 3. Non-Goals

- Full git workflow (commit, push, branching, merge conflict resolution)
- Web-based git UI or diff viewer
- Repository creation on remote platforms
- Platform-specific API features (issues, PRs, CI pipelines)

## 4. Configuration Changes

### 4.1 New `source` property on `RepositoryConfig`

A new optional `source` block on each repository entry describes where to clone from. When `source` is present and `path` points to a non-existent directory, Amalgam clones the repo before proceeding.

```yaml
repositories:
  # Existing style — local only, no change
  - name: my-local-service
    type: microservice
    path: C:\repos\my-local-service

  # New style — remote source
  - name: order-service
    type: microservice
    path: C:\repos\order-service
    source:
      url: https://github.com/acme/order-service.git
      branch: main            # optional, default: remote HEAD
      credential: github-pat  # optional, references a named credential

  # Self-hosted GitLab
  - name: shared-lib
    type: library
    path: C:\repos\shared-lib
    source:
      url: https://git.scm.internal.corp/platform/shared-lib.git
      branch: develop
      credential: corp-gitlab

  # SSH-based
  - name: ui-dashboard
    type: dashboard
    path: C:\repos\ui-dashboard
    source:
      url: git@bitbucket.org:acme/ui-dashboard.git
```

### 4.2 New top-level `credentials` section

Credentials are defined once and referenced by name. Amalgam never stores raw secrets in the config file; instead it references environment variables or the system credential store.

```yaml
credentials:
  github-pat:
    type: token
    env: GITHUB_TOKEN               # reads from environment variable

  corp-gitlab:
    type: token
    env: CORP_GITLAB_TOKEN

  bitbucket-app:
    type: app-password
    env: BITBUCKET_APP_PASSWORD
    username-env: BITBUCKET_USERNAME # Bitbucket app passwords need a username
```

Supported credential types:

| Type            | Fields                      | Description                                        |
|-----------------|-----------------------------|----------------------------------------------------|
| `token`         | `env`                       | Personal access token injected into HTTPS URL       |
| `app-password`  | `env`, `username-env`       | Bitbucket app passwords (username + password pair)  |
| `ssh`           | `key-path` (optional)       | Uses SSH agent or explicit key file                 |
| `helper`        | (none)                      | Delegates to `git credential-helper` (system default)|

When no credential is specified on a `source`, Amalgam falls back to the system's default git credential resolution (SSH agent, credential manager, etc.).

### 4.3 Updated `AmalgamConfig` model

```
AmalgamConfig
├── Repositories: List<RepositoryConfig>   (existing)
├── Backend: BackendConfig                 (existing)
├── Frontend: FrontendConfig               (existing)
└── Credentials: Dictionary<string, CredentialConfig>  (NEW)
```

### 4.4 Updated `RepositoryConfig` model

```
RepositoryConfig
├── Name           (existing)
├── Type           (existing)
├── Path           (existing)
├── Enabled        (existing)
├── RoutePrefix    (existing)
├── PackageName    (existing)
├── Merge          (existing)
└── Source: GitSourceConfig?  (NEW, optional)
```

## 5. New Core Types

### 5.1 `GitSourceConfig`

```csharp
// Amalgam.Core/Git/GitSourceConfig.cs
public class GitSourceConfig
{
    public string Url { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public string? Credential { get; set; }  // references key in credentials dict
}
```

### 5.2 `CredentialConfig`

```csharp
// Amalgam.Core/Git/CredentialConfig.cs
public class CredentialConfig
{
    public CredentialType Type { get; set; } = CredentialType.Helper;
    public string? Env { get; set; }          // env var holding the token/password
    public string? UsernameEnv { get; set; }  // env var holding username (Bitbucket)
    public string? KeyPath { get; set; }       // explicit SSH key path
}

public enum CredentialType
{
    Token,
    AppPassword,
    Ssh,
    Helper
}
```

### 5.3 `IGitOperations` — Platform-Agnostic Interface

All git operations go through a single interface. The implementation shells out to the `git` CLI, making it work with any hosting platform.

```csharp
// Amalgam.Core/Git/IGitOperations.cs
public interface IGitOperations
{
    GitResult Clone(GitSourceConfig source, string targetPath,
                    CredentialConfig? credential);
    GitResult Pull(string repoPath, string? branch,
                   CredentialConfig? credential);
    GitResult Fetch(string repoPath, CredentialConfig? credential);
    bool IsGitRepository(string path);
    string? GetCurrentBranch(string repoPath);
    string? GetRemoteUrl(string repoPath);
}

public class GitResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int ExitCode { get; set; }
}
```

### 5.4 `GitCliOperations` — Implementation

The single implementation works for **all** platforms (GitHub, GitLab, Bitbucket, self-hosted) because it delegates to the `git` CLI and manipulates the remote URL to inject credentials when needed.

```csharp
// Amalgam.Core/Git/GitCliOperations.cs
public class GitCliOperations : IGitOperations
{
    // For HTTPS token auth: rewrites
    //   https://host/path.git  →  https://token@host/path.git
    // For app-password auth: rewrites
    //   https://host/path.git  →  https://user:password@host/path.git
    // For SSH / Helper: passes URL unmodified

    public GitResult Clone(GitSourceConfig source, string targetPath,
                           CredentialConfig? credential)
    {
        var url = ResolveAuthenticatedUrl(source.Url, credential);
        var args = $"clone {url} \"{targetPath}\"";
        if (source.Branch != null)
            args = $"clone -b {source.Branch} {url} \"{targetPath}\"";
        return RunGit(args);
    }

    // Pull, Fetch, IsGitRepository, etc. follow the same pattern
}
```

Key design point: **no platform-specific subclasses**. The `git` CLI protocol is the same for GitHub, GitLab, Bitbucket, and self-hosted instances. The only variation is authentication, which is handled by URL rewriting and credential resolution.

## 6. New Service: `GitSyncService`

Orchestrates clone/pull across all configured repositories.

```csharp
// Amalgam.Core/Git/GitSyncService.cs
public class GitSyncService
{
    private readonly IGitOperations _git;

    public GitSyncService(IGitOperations git) { _git = git; }

    public List<SyncResult> SyncAll(AmalgamConfig config)
    {
        var results = new List<SyncResult>();
        foreach (var repo in config.Repositories.Where(r => r.Enabled && r.Source != null))
        {
            var credential = ResolveCredential(repo.Source!.Credential, config.Credentials);
            if (!Directory.Exists(repo.Path))
                // Clone
            else if (_git.IsGitRepository(repo.Path))
                // Pull
            results.Add(result);
        }
        return results;
    }

    private CredentialConfig? ResolveCredential(string? name,
        Dictionary<string, CredentialConfig> credentials)
    {
        if (name == null) return null;
        return credentials.TryGetValue(name, out var cred) ? cred : null;
    }
}

public class SyncResult
{
    public string RepositoryName { get; set; } = string.Empty;
    public SyncAction Action { get; set; }   // Cloned, Pulled, Skipped, Failed
    public string Message { get; set; } = string.Empty;
}
```

## 7. Changes to Existing Components

### 7.1 `ConfigValidator` — New Validations

Add validation rules:
- If `source.credential` is specified, a matching entry must exist in `credentials`
- If `source.url` is present, it must be a valid git URL (HTTPS or SSH)
- If credential type is `token` or `app-password`, the referenced env var must be set
- Warn (not error) if `path` does not exist but `source` is provided (clone will create it)

### 7.2 `DirectoryScanner` — Enhanced Detection

When scanning, detect existing git remotes and populate the `source` block automatically:

```csharp
// In DirectoryScanner.DetectRepository:
var remoteUrl = gitOps.GetRemoteUrl(dir.FullName);
if (remoteUrl != null)
{
    repo.Source = new GitSourceConfig { Url = remoteUrl };
}
```

### 7.3 CLI — New `sync` Command

```
amalgam sync [--only <name>] [--force]
```

- Clones repositories that have a `source` but no local directory
- Pulls latest for repositories that have a `source` and exist locally
- `--only <name>` limits to a single repository
- `--force` resets local changes before pulling

### 7.4 CLI — Enhanced `init` Command

When `--scan-dir` discovers git repos, auto-populate `source.url` from the remote origin.

### 7.5 CLI — Enhanced `run` Command

Before building/running, optionally auto-sync if `source` is configured and the directory is missing. Controlled by a `--no-sync` flag to skip.

### 7.6 API — New `SyncController`

```
POST /api/sync           — sync all repositories
POST /api/sync/{name}    — sync a single repository
```

Returns a list of `SyncResult` objects.

### 7.7 Frontend — Changes

| Library      | Change                                                              |
|-------------|----------------------------------------------------------------------|
| `api`       | Add `GitSourceConfig`, `CredentialConfig` models; add `SyncService` |
| `components`| Add `SourceConfigForm` presentation component                        |
| `domain`    | Update `AddRepositoryPage` and `RepositoryDetailPage` to include source config; add `SyncPage` |

## 8. File Layout (New Files)

```
src/Amalgam.Core/Git/
├── GitSourceConfig.cs
├── CredentialConfig.cs
├── CredentialType.cs
├── IGitOperations.cs
├── GitCliOperations.cs
├── GitResult.cs
├── GitSyncService.cs
└── SyncResult.cs

src/Amalgam.Api/Controllers/
└── SyncController.cs          (new)

src/Amalgam.Web/projects/api/src/lib/
├── models/
│   ├── git-source-config.ts   (new)
│   ├── credential-config.ts   (new)
│   └── sync-result.ts         (new)
└── services/
    └── sync.service.ts        (new)

src/Amalgam.Web/projects/components/src/lib/
└── source-config-form/        (new)

src/Amalgam.Web/projects/domain/src/lib/
└── sync/                      (new)

tests/Amalgam.Tests/Git/
├── GitCliOperationsTests.cs   (new)
└── GitSyncServiceTests.cs     (new)
```

## 9. Authentication Flow

```
User configures amalgam.yml
        │
        ▼
  source.credential ──► references credentials[name]
        │
        ▼
  CredentialConfig.Type?
        │
  ┌─────┼──────────┬──────────┐
  ▼     ▼          ▼          ▼
Token  AppPassword  SSH      Helper
  │     │           │          │
  │     │           │          └─► Pass URL unchanged,
  │     │           │              git uses system helper
  │     │           │
  │     │           └─► Use URL as-is (git@...),
  │     │               set GIT_SSH_COMMAND if key-path
  │     │
  │     └─► Read username from env, password from env
  │         Rewrite URL: https://user:pass@host/path
  │
  └─► Read token from env
      Rewrite URL: https://token@host/path
```

## 10. Platform Compatibility Matrix

| Platform              | HTTPS + Token | HTTPS + App Password | SSH  | System Helper |
|-----------------------|:---:|:---:|:---:|:---:|
| GitHub                | Y   | —   | Y   | Y   |
| GitLab (cloud)        | Y   | —   | Y   | Y   |
| GitLab (self-hosted)  | Y   | —   | Y   | Y   |
| Bitbucket Cloud       | Y   | Y   | Y   | Y   |
| Bitbucket Server      | Y   | —   | Y   | Y   |
| Generic git host      | Y   | —   | Y   | Y   |

## 11. Security Considerations

1. **No secrets in config** — tokens are read from environment variables at runtime
2. **URL credential injection is ephemeral** — authenticated URLs are constructed in memory for the git CLI call, never written to disk
3. **`GIT_TERMINAL_PROMPT=0`** — set on all git subprocess calls to prevent interactive password prompts that would hang the process
4. **Credential env var validation** — `ConfigValidator` warns at validation time if a referenced env var is not set
5. **SSH key permissions** — if `key-path` is specified, validate the file exists and warn if permissions are too open (non-Windows)

## 12. Backward Compatibility

- The `source` field is optional. Existing configs with only `path` are fully supported
- The `credentials` section is optional. Configs without it continue to work
- `ConfigValidator` relaxes the "path must exist" rule when `source` is present (the path will be created by clone)
- No changes to the existing `init`, `validate`, `build`, `run`, `status` behavior unless `source` is present

## 13. Implementation Phases

### Phase 1: Core git abstraction
- Add `GitSourceConfig`, `CredentialConfig`, `CredentialType` models
- Add `credentials` to `AmalgamConfig`
- Add `source` to `RepositoryConfig`
- Implement `IGitOperations` / `GitCliOperations`
- Update `ConfigValidator` for new fields
- Unit tests

### Phase 2: Sync service and CLI
- Implement `GitSyncService`
- Add `amalgam sync` CLI command
- Enhance `init --scan-dir` to detect remotes
- Integration tests

### Phase 3: API and frontend
- Add `SyncController`
- Add frontend models, services, and UI components
- Update repository forms to include source configuration
- Add sync page to web UI

### Phase 4: Enhanced developer experience
- Auto-sync on `amalgam run` when directories are missing
- Parallel clone/pull for faster sync
- Progress reporting for long-running clone operations
- `amalgam status` shows sync state (ahead/behind/dirty)
