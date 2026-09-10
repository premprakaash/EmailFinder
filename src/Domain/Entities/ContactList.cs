using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class ContactList : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ContactListItem> Items { get; set; } = [];
}
