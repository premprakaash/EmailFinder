using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class VerificationResult : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? EmailAddressId { get; set; }
    public string Email { get; set; } = string.Empty;
    public EmailStatus Status { get; set; }
    public int Score { get; set; }
    public bool MxValid { get; set; }
    public bool SmtpValid { get; set; }
    public bool CatchAll { get; set; }
    public bool Disposable { get; set; }
    public bool RoleBased { get; set; }
    public string? Provider { get; set; }
    public string? Details { get; set; }

    public User User { get; set; } = null!;
    public EmailAddress? EmailAddressEntity { get; set; }
}
