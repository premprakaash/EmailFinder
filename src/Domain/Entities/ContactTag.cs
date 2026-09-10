using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class ContactTag : BaseEntity
{
    public Guid ContactId { get; set; }
    public Guid TagId { get; set; }

    public Contact Contact { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
