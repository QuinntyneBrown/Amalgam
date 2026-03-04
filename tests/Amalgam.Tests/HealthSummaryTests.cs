using Amalgam.Core.Modules;
using Amalgam.Host;

namespace Amalgam.Tests;

public class HealthSummaryTests
{
    [Fact]
    public void RenderToString_ContainsServiceNames()
    {
        var results = new List<ModuleLoadResult>
        {
            new() { ServiceName = "users-api", Status = ModuleLoadStatus.Loaded, RoutePrefix = "/users", LoadTime = TimeSpan.FromMilliseconds(42) },
            new() { ServiceName = "orders-api", Status = ModuleLoadStatus.Error, RoutePrefix = "/orders", LoadTime = TimeSpan.FromMilliseconds(5), ErrorMessage = "Missing assembly" }
        };

        var output = HealthSummary.RenderToString(results);

        Assert.Contains("users-api", output);
        Assert.Contains("orders-api", output);
    }

    [Fact]
    public void RenderToString_ContainsStatusLabels()
    {
        var results = new List<ModuleLoadResult>
        {
            new() { ServiceName = "svc-a", Status = ModuleLoadStatus.Loaded, RoutePrefix = "/a", LoadTime = TimeSpan.Zero },
            new() { ServiceName = "svc-b", Status = ModuleLoadStatus.Skipped, RoutePrefix = "/b", LoadTime = TimeSpan.Zero },
            new() { ServiceName = "svc-c", Status = ModuleLoadStatus.Error, RoutePrefix = "/c", LoadTime = TimeSpan.Zero, ErrorMessage = "fail" }
        };

        var output = HealthSummary.RenderToString(results);

        Assert.Contains("Loaded", output);
        Assert.Contains("Skipped", output);
        Assert.Contains("Error", output);
    }

    [Fact]
    public void RenderToString_ContainsTableHeader()
    {
        var results = new List<ModuleLoadResult>
        {
            new() { ServiceName = "test", Status = ModuleLoadStatus.Loaded, RoutePrefix = "/test", LoadTime = TimeSpan.Zero }
        };

        var output = HealthSummary.RenderToString(results);

        Assert.Contains("Service", output);
        Assert.Contains("Status", output);
        Assert.Contains("Route Prefix", output);
        Assert.Contains("Load Time", output);
    }
}
