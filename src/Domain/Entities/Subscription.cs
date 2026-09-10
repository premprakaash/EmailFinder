using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}
