using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class ApiKey : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public long RequestCount { get; set; }

    public User User { get; set; } = null!;
}
