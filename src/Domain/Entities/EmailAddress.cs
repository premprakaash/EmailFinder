using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class EmailAddress : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public EmailStatus Status { get; set; } = EmailStatus.Unknown;
    public int Score { get; set; }
    public bool MxValid { get; set; }
    public bool SmtpValid { get; set; }
    public bool CatchAll { get; set; }
    public bool Disposable { get; set; }
    public bool RoleBased { get; set; }
    public DateTime? LastVerifiedAt { get; set; }

    public ICollection<VerificationResult> VerificationResults { get; set; } = [];
}
