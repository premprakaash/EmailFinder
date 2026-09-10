using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class FinderResult : BaseEntity
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string Email { get; set; } = string.Empty;
    public int ConfidenceScore { get; set; }
    public EmailStatus Status { get; set; }
    public string? Provider { get; set; }

    public User User { get; set; } = null!;
}
