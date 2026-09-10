using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class VerificationJob : BaseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public EmailStatus? ResultStatus { get; set; }
    public int? Score { get; set; }
    public string? ErrorMessage { get; set; }

    public User User { get; set; } = null!;
}
