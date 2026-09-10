using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class Tag : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";

    public User User { get; set; } = null!;
    public ICollection<ContactTag> ContactTags { get; set; } = [];
}
