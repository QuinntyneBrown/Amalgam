using Amalgam.Host.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Amalgam.Tests;

public class RoutePrefixTests
{
    [Fact]
    public async Task RoutePrefix_StripsPrefix()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.UseRoutePrefix("api");
                    app.Run(async ctx =>
                    {
                        await ctx.Response.WriteAsync($"path={ctx.Request.Path},base={ctx.Request.PathBase}");
                    });
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetStringAsync("/api/hello");

        Assert.Contains("path=/hello", response);
        Assert.Contains("base=/api", response);
    }

    [Fact]
    public async Task RoutePrefix_PassesThroughNonMatchingPaths()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.UseRoutePrefix("api");
                    app.Run(async ctx =>
                    {
                        await ctx.Response.WriteAsync($"path={ctx.Request.Path}");
                    });
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetStringAsync("/other/path");

        Assert.Contains("path=/other/path", response);
    }
}
