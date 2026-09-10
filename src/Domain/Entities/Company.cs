using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public int? EmployeeCount { get; set; }

    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<DomainEntity> Domains { get; set; } = [];
}
