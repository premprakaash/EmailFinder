using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class SearchJob : BaseEntity
{
    public Guid UserId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public int TotalFound { get; set; }
    public string? Provider { get; set; }
    public string? ErrorMessage { get; set; }

    public User User { get; set; } = null!;
}
