using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class Export : BaseEntity
{
    public Guid UserId { get; set; }
    public string Format { get; set; } = "csv";
    public int ContactCount { get; set; }
    public long CreditsConsumed { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string DownloadToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? DownloadedAt { get; set; }
    public string? Filters { get; set; }

    public User User { get; set; } = null!;
}
