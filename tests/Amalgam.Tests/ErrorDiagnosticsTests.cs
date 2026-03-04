using System.Reflection;
using Amalgam.Host.Diagnostics;

namespace Amalgam.Tests;

public class ErrorDiagnosticsTests
{
    [Fact]
    public void Diagnose_FileNotFoundException_SuggestsBuild()
    {
        var ex = new FileNotFoundException("Could not find file.", "MyService.dll");

        var message = ErrorDiagnostics.Diagnose(ex);

        Assert.Contains("amalgam build", message);
        Assert.Contains("MyService.dll", message);
    }

    [Fact]
    public void Diagnose_ReflectionTypeLoadException_ListsLoaderExceptions()
    {
        var loaderExceptions = new Exception[]
        {
            new TypeLoadException("Could not load type 'Foo'"),
            new FileNotFoundException("Missing dep.dll")
        };
        var ex = new ReflectionTypeLoadException(Array.Empty<Type?>(), loaderExceptions);

        var message = ErrorDiagnostics.Diagnose(ex);

        Assert.Contains("Foo", message);
        Assert.Contains("dep.dll", message);
    }

    [Fact]
    public void Diagnose_DIConflict_IdentifiesConflict()
    {
        var ex = new InvalidOperationException("Unable to resolve service for type 'IFoo'");

        var message = ErrorDiagnostics.Diagnose(ex);

        Assert.Contains("DI registration conflict", message);
    }

    [Fact]
    public void Diagnose_UnknownException_ReturnsGenericMessage()
    {
        var ex = new ArgumentException("bad arg");

        var message = ErrorDiagnostics.Diagnose(ex);

        Assert.Contains("Module load failed", message);
        Assert.Contains("bad arg", message);
    }
}
