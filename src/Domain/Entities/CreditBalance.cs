using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class CreditBalance : BaseEntity
{
    public Guid UserId { get; set; }
    public long Balance { get; set; }
    public long TotalPurchased { get; set; }
    public long TotalConsumed { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public User User { get; set; } = null!;
}
