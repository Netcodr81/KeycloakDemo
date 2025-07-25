using System.Text.Json;

namespace Blazor.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var route = context.Request.Path;
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var method = context.Request.Method;
        var parameters = context.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
        var body = "";

        if (method == "POST" || method == "PUT")
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        _logger.LogInformation("Route: {Route}, Method: {Method}, Query: {Query}, Parameters: {Parameters}, Body: {Body}",
            route, method, query, JsonSerializer.Serialize(parameters), body);

        await _next(context);
    }
}

