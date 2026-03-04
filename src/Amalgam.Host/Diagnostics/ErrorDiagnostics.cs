using System.Reflection;

namespace Amalgam.Host.Diagnostics;

public static class ErrorDiagnostics
{
    public static string Diagnose(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException fnf =>
                $"Assembly not found: {fnf.FileName}. Run 'amalgam build' first.",

            ReflectionTypeLoadException rtle =>
                $"Failed to load types from assembly. Loader exceptions:\n" +
                string.Join("\n", rtle.LoaderExceptions
                    .Where(e => e is not null)
                    .Select(e => $"  - {e!.Message}")),

            InvalidOperationException ioe when ioe.Message.Contains("service") =>
                $"DI registration conflict: {ioe.Message}",

            _ => $"Module load failed: {ex.Message}"
        };
    }
}
