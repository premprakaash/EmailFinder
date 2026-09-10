using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class RateLimitConfig : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Scope { get; set; } = "user";
    public string? Endpoint { get; set; }
    public int RequestsPerMinute { get; set; } = 600;
    public int RequestsPerDay { get; set; } = 10000;
    public bool IsActive { get; set; } = true;
}
