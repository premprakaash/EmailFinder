using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public bool IsSuspended { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid? OrganizationId { get; set; }

    public Organization? Organization { get; set; }
    public CreditBalance? CreditBalance { get; set; }
    public Subscription? Subscription { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<ApiKey> ApiKeys { get; set; } = [];
    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<Export> Exports { get; set; } = [];
    public ICollection<UsageRecord> UsageRecords { get; set; } = [];
}
