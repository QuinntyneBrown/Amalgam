using System.CommandLine;
using System.Diagnostics;
using Amalgam.Core.Configuration;
using Amalgam.Core.Plugins;
using Amalgam.Host;
using Spectre.Console;

var rootCommand = new RootCommand("Amalgam - Unified development environment manager");

// init command
var initCommand = new Command("init", "Initialize a new amalgam.yml configuration");
var scanDirOption = new Option<string?>("--scan-dir", "Directory to scan for repositories");
initCommand.AddOption(scanDirOption);
initCommand.SetHandler(HandleInit, scanDirOption);

// validate command
var validateCommand = new Command("validate", "Validate the amalgam.yml configuration");
validateCommand.SetHandler(HandleValidate);

// build command
var buildCommand = new Command("build", "Build all enabled backend repositories");
buildCommand.SetHandler(HandleBuild);

// run command
var runCommand = new Command("run", "Run the unified development environment");
var portOption = new Option<int?>("--port", "Override the backend port");
var enableOption = new Option<string[]>("--enable", "Enable specific repositories") { AllowMultipleArgumentsPerToken = true };
var disableOption = new Option<string[]>("--disable", "Disable specific repositories") { AllowMultipleArgumentsPerToken = true };
var envOption = new Option<string[]>("--env", "Set environment variables (KEY=VALUE)") { AllowMultipleArgumentsPerToken = true };
var verboseOption = new Option<bool>("--verbose", "Enable verbose output");
runCommand.AddOption(portOption);
runCommand.AddOption(enableOption);
runCommand.AddOption(disableOption);
runCommand.AddOption(envOption);
runCommand.AddOption(verboseOption);
runCommand.SetHandler(HandleRun, portOption, enableOption, disableOption, envOption, verboseOption);

// link-plugins command
var linkPluginsCommand = new Command("link-plugins", "Link plugin packages for local development");
var onlyOption = new Option<string?>("--only", "Link only a specific plugin by name");
linkPluginsCommand.AddOption(onlyOption);
linkPluginsCommand.SetHandler(HandleLinkPlugins, onlyOption);

// unlink-plugins command
var unlinkPluginsCommand = new Command("unlink-plugins", "Unlink plugin packages");
unlinkPluginsCommand.SetHandler(HandleUnlinkPlugins);

// status command
var statusCommand = new Command("status", "Show status of all repositories");
statusCommand.SetHandler(HandleStatus);

rootCommand.AddCommand(initCommand);
rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(buildCommand);
rootCommand.AddCommand(runCommand);
rootCommand.AddCommand(linkPluginsCommand);
rootCommand.AddCommand(unlinkPluginsCommand);
rootCommand.AddCommand(statusCommand);

return await rootCommand.InvokeAsync(args);

// --- Handler implementations ---

static void HandleInit(string? scanDir)
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");

    AmalgamConfig config;
    if (scanDir != null)
    {
        var fullPath = Path.GetFullPath(scanDir);
        AnsiConsole.MarkupLine($"[blue]Scanning directory:[/] {fullPath}");
        config = DirectoryScanner.Scan(fullPath);
        AnsiConsole.MarkupLine($"[green]Found {config.Repositories.Count} repositories[/]");
    }
    else
    {
        config = new AmalgamConfig();
    }

    ConfigLoader.Save(config, configPath);
    AnsiConsole.MarkupLine($"[green]Configuration written to:[/] {configPath}");
}

static void HandleValidate()
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");
    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine("[red]No amalgam.yml found. Run 'amalgam init' first.[/]");
        return;
    }

    var config = ConfigLoader.Load(configPath);
    var errors = ConfigValidator.Validate(config);

    if (errors.Count == 0)
    {
        AnsiConsole.MarkupLine("[green]Configuration is valid.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]Found {errors.Count} validation error(s):[/]");
        foreach (var error in errors)
        {
            AnsiConsole.MarkupLine($"  [red]• {Markup.Escape(error)}[/]");
        }
    }
}

static void HandleBuild()
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");
    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine("[red]No amalgam.yml found. Run 'amalgam init' first.[/]");
        return;
    }

    var config = ConfigLoader.Load(configPath);
    var backendRepos = config.Repositories
        .Where(r => r.Enabled && (r.Type == RepositoryType.Microservice || r.Type == RepositoryType.Library))
        .ToList();

    if (backendRepos.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No enabled backend repositories to build.[/]");
        return;
    }

    foreach (var repo in backendRepos)
    {
        AnsiConsole.MarkupLine($"[blue]Building {Markup.Escape(repo.Name)}...[/]");
        var psi = new ProcessStartInfo("dotnet", "build")
        {
            WorkingDirectory = repo.Path,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var process = Process.Start(psi);
        if (process == null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to start build for {Markup.Escape(repo.Name)}[/]");
            continue;
        }
        process.WaitForExit();
        if (process.ExitCode == 0)
        {
            AnsiConsole.MarkupLine($"[green]  {Markup.Escape(repo.Name)} built successfully[/]");
        }
        else
        {
            var stderr = process.StandardError.ReadToEnd();
            AnsiConsole.MarkupLine($"[red]  {Markup.Escape(repo.Name)} build failed (exit code {process.ExitCode})[/]");
            if (!string.IsNullOrWhiteSpace(stderr))
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(stderr)}[/]");
        }
    }
}

static async Task HandleRun(int? port, string[] enable, string[] disable, string[] env, bool verbose)
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");
    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine("[red]No amalgam.yml found. Run 'amalgam init' first.[/]");
        return;
    }

    var config = ConfigLoader.Load(configPath);

    // Apply overrides
    if (port.HasValue)
        config.Backend.Port = port.Value;

    if (enable.Length > 0)
    {
        foreach (var name in enable)
        {
            var repo = config.Repositories.FirstOrDefault(r =>
                r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (repo != null) repo.Enabled = true;
        }
    }

    if (disable.Length > 0)
    {
        foreach (var name in disable)
        {
            var repo = config.Repositories.FirstOrDefault(r =>
                r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (repo != null) repo.Enabled = false;
        }
    }

    foreach (var kvp in env)
    {
        var parts = kvp.Split('=', 2);
        if (parts.Length == 2)
            config.Backend.Environment[parts[0]] = parts[1];
    }

    if (verbose)
    {
        AnsiConsole.MarkupLine($"[blue]Backend port:[/] {config.Backend.Port}");
        AnsiConsole.MarkupLine($"[blue]Enabled repos:[/] {string.Join(", ", config.Repositories.Where(r => r.Enabled).Select(r => r.Name))}");
    }

    // Set environment variables
    foreach (var kvp in config.Backend.Environment)
        Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);

    // Build and run the unified host
    var hostBuilder = new AmalgamHostBuilder(config);
    var hostArgs = new List<string> { "--urls", $"http://localhost:{config.Backend.Port}" };
    var buildResult = hostBuilder.Build(hostArgs.ToArray());

    // Print health summary
    HealthSummary.Print(buildResult.Results);

    AnsiConsole.MarkupLine($"\n[green]Amalgam host starting on port {config.Backend.Port}...[/]");
    await buildResult.App.RunAsync();
}

static void HandleStatus()
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");
    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine("[red]No amalgam.yml found. Run 'amalgam init' first.[/]");
        return;
    }

    var config = ConfigLoader.Load(configPath);

    var table = new Table();
    table.AddColumn("Name");
    table.AddColumn("Type");
    table.AddColumn("Enabled");
    table.AddColumn("Path");

    foreach (var repo in config.Repositories)
    {
        var enabledText = repo.Enabled ? "[green]Yes[/]" : "[red]No[/]";
        table.AddRow(
            Markup.Escape(repo.Name),
            repo.Type.ToString(),
            enabledText,
            Markup.Escape(repo.Path));
    }

    AnsiConsole.Write(table);
}

static void HandleLinkPlugins(string? only)
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");
    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine("[red]No amalgam.yml found. Run 'amalgam init' first.[/]");
        return;
    }

    var config = ConfigLoader.Load(configPath);
    var service = new NpmLinkService();
    var results = service.LinkPlugins(config, only);

    if (results.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No plugins found to link.[/]");
        return;
    }

    foreach (var result in results)
    {
        if (result.Success)
            AnsiConsole.MarkupLine($"[green]Linked {Markup.Escape(result.PluginName)}[/]");
        else
            AnsiConsole.MarkupLine($"[red]Failed to link {Markup.Escape(result.PluginName)}: {Markup.Escape(result.Error ?? "unknown error")}[/]");
    }
}

static void HandleUnlinkPlugins()
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "amalgam.yml");
    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine("[red]No amalgam.yml found. Run 'amalgam init' first.[/]");
        return;
    }

    var config = ConfigLoader.Load(configPath);
    var service = new NpmLinkService();
    var results = service.UnlinkPlugins(config);

    if (results.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No plugins found to unlink.[/]");
        return;
    }

    foreach (var result in results)
    {
        if (result.Success)
            AnsiConsole.MarkupLine($"[green]Unlinked {Markup.Escape(result.PluginName)}[/]");
        else
            AnsiConsole.MarkupLine($"[red]Failed to unlink {Markup.Escape(result.PluginName)}: {Markup.Escape(result.Error ?? "unknown error")}[/]");
    }

    AnsiConsole.MarkupLine("[green]Restored node_modules via npm install.[/]");
}
