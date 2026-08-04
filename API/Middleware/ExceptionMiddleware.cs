using System.Net;
using System.Text.Json;
using Core.Exceptions;
using ValidationException = Core.Validations.ValidationException;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                validationException.Message,
                (object?)validationException.Errors),

            UnauthorizedException unauthorizedException => (
                (int)HttpStatusCode.Unauthorized,
                unauthorizedException.Message,
                null),

            ConflictException conflictException => (
                (int)HttpStatusCode.Conflict,
                conflictException.Message,
                null),

            KeyNotFoundException keyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                keyNotFoundException.Message,
                null),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                null)
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message,
            errors
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
