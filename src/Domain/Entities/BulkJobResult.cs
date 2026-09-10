using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class BulkJobResult : BaseEntity
{
    public Guid BulkJobId { get; set; }
    public int RowNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? ConfidenceScore { get; set; }
    public EmailStatus? Status { get; set; }
    public string? ErrorMessage { get; set; }

    public BulkJob BulkJob { get; set; } = null!;
}
