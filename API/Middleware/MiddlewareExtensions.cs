using System.Security.Claims;
using API.Middleware;
using Serilog;
using Serilog.Events;

namespace API;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, exception) =>
            {
                var path = httpContext.Request.Path.Value ?? string.Empty;
                if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
                {
                    return LogEventLevel.Verbose;
                }

                if (exception is not null || httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                return LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("CorrelationId",
                    httpContext.Items[CorrelationIdMiddleware.HeaderName]?.ToString()
                    ?? httpContext.TraceIdentifier);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId",
                        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    diagnosticContext.Set("UserRole",
                        httpContext.User.FindFirstValue(ClaimTypes.Role));
                }
            };
        });
    }
}
