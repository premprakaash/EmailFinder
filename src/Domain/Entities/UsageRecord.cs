using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class UsageRecord : BaseEntity
{
    public Guid UserId { get; set; }
    public CreditOperation Operation { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public long CreditsUsed { get; set; }
    public string? IpAddress { get; set; }
    public Guid? ApiKeyId { get; set; }
    public int ResponseStatus { get; set; }
    public long DurationMs { get; set; }

    public User User { get; set; } = null!;
}
