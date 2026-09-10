namespace MailForge.Application.Interfaces;

public interface IRateLimitService
{
    Task<bool> IsAllowedAsync(string key, string scope, string? endpoint = null, CancellationToken cancellationToken = default);
}
