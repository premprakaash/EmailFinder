using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class ProviderConfig : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Settings { get; set; }
}
