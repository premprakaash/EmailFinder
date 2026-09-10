using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class BulkJob : BaseEntity
{
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int FoundCount { get; set; }
    public int VerifiedCount { get; set; }
    public int InvalidCount { get; set; }
    public int RiskyCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<BulkJobResult> Results { get; set; } = [];
}
