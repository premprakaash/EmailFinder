using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class Contact : BaseEntity
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Domain { get; set; }
    public string? Country { get; set; }
    public string? Industry { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Unknown;
    public int ConfidenceScore { get; set; }
    public string? Source { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? DomainEntityId { get; set; }

    public User User { get; set; } = null!;
    public Company? CompanyEntity { get; set; }
    public DomainEntity? DomainEntity { get; set; }
    public ICollection<ContactTag> ContactTags { get; set; } = [];
    public ICollection<ContactListItem> ContactListItems { get; set; } = [];
}
