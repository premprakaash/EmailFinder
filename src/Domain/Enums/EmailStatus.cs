namespace MailForge.Domain.Enums;

public enum EmailStatus
{
    Valid = 0,
    Invalid = 1,
    Risky = 2,
    Unknown = 3,
    CatchAll = 4,
    Disposable = 5,
    RoleBased = 6
}
