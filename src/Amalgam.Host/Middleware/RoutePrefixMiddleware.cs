using Microsoft.AspNetCore.Http;

namespace Amalgam.Host.Middleware;

/// <summary>
/// Middleware that strips a route prefix from incoming requests so that
/// downstream middleware and endpoints see a root-relative path.
/// Used when modules are mounted under /{prefix}/.
/// </summary>
public class RoutePrefixMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PathString _prefix;

    public RoutePrefixMiddleware(RequestDelegate next, string prefix)
    {
        _next = next;
        _prefix = new PathString("/" + prefix.TrimStart('/'));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalPath = context.Request.Path;
        var originalPathBase = context.Request.PathBase;

        if (originalPath.StartsWithSegments(_prefix, out var remaining))
        {
            context.Request.PathBase = originalPathBase.Add(_prefix);
            context.Request.Path = remaining;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            context.Request.PathBase = originalPathBase;
            context.Request.Path = originalPath;
        }
    }
}

public static class RoutePrefixMiddlewareExtensions
{
    public static IApplicationBuilder UseRoutePrefix(this IApplicationBuilder app, string prefix)
    {
        return app.UseMiddleware<RoutePrefixMiddleware>(prefix);
    }
}
