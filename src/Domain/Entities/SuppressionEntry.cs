using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class SuppressionEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
