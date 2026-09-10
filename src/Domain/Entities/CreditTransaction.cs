using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class CreditTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public CreditOperation Operation { get; set; }
    public long Amount { get; set; }
    public long BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }

    public User User { get; set; } = null!;
}
