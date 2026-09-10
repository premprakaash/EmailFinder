using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class ContactListItem : BaseEntity
{
    public Guid ContactListId { get; set; }
    public Guid ContactId { get; set; }

    public ContactList ContactList { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
