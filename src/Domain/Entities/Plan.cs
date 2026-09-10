using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public long MonthlyCredits { get; set; }
    public int RateLimitPerMinute { get; set; } = 600;
    public int RateLimitPerDay { get; set; } = 10000;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
