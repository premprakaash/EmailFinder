using MailForge.Application.Interfaces;
using System.Security.Claims;

namespace MailForge.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService, IRateLimitService rateLimitService)
    {
        if (!context.Request.Path.StartsWithSegments("/api/v1"))
        {
            await _next(context);
            return;
        }

        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await _next(context);
            return;
        }

        var userId = await apiKeyService.ValidateKeyAsync(apiKey, context.RequestAborted);
        if (userId == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Path.Value;
        if (!await rateLimitService.IsAllowedAsync(userId.Value.ToString(), "apikey", endpoint, context.RequestAborted))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded" });
            return;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(ClaimTypes.Role, "User")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "ApiKey"));
        context.Items["AuthMethod"] = "ApiKey";
        await _next(context);
    }
}
