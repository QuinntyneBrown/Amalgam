using Amalgam.Core.Modules;
using Spectre.Console;

namespace Amalgam.Host;

public static class HealthSummary
{
    public static void Print(IReadOnlyList<ModuleLoadResult> results, IAnsiConsole? console = null)
    {
        console ??= AnsiConsole.Console;

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Title("[bold]Amalgam Startup Summary[/]");

        table.AddColumn("Service");
        table.AddColumn("Status");
        table.AddColumn("Route Prefix");
        table.AddColumn("Load Time");

        foreach (var result in results)
        {
            var statusMarkup = result.Status switch
            {
                ModuleLoadStatus.Loaded => "[green]Loaded[/]",
                ModuleLoadStatus.Skipped => "[yellow]Skipped[/]",
                ModuleLoadStatus.Error => "[red]Error[/]",
                _ => result.Status.ToString()
            };

            var routePrefix = result.RoutePrefix ?? "-";
            var loadTime = $"{result.LoadTime.TotalMilliseconds:F1}ms";

            table.AddRow(
                Markup.Escape(result.ServiceName),
                statusMarkup,
                Markup.Escape(routePrefix),
                loadTime);

            if (result.Status == ModuleLoadStatus.Error && result.ErrorMessage is not null)
            {
                table.AddRow(
                    string.Empty,
                    $"[red]{Markup.Escape(result.ErrorMessage)}[/]",
                    string.Empty,
                    string.Empty);
            }
        }

        console.Write(table);
    }

    public static string RenderToString(IReadOnlyList<ModuleLoadResult> results)
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(writer)
        });
        Print(results, console);
        return writer.ToString();
    }
}
