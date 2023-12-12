using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = env;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception: {Message}", exception.Message);

            httpContext.Response.ContentType = "application/problem+json; charset=utf-8";
            httpContext.Response.StatusCode = 500;

            var response = new ProblemDetails
            {
                Status = 500,
                Detail = _env.IsDevelopment() ? exception.StackTrace?.ToString() : null,
                Title = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response, _jsonSerializerOptions);

            await httpContext.Response.WriteAsync(jsonResponse);
        }
    }
}
