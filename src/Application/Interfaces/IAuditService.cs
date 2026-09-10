namespace MailForge.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, CancellationToken cancellationToken = default);
}
