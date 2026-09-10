using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Infrastructure.Persistence;

namespace MailForge.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly MailForgeDbContext _db;

    public AuditService(MailForgeDbContext db) => _db = db;

    public async Task LogAsync(Guid? userId, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
