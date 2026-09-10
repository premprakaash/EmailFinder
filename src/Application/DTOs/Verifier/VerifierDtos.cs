using MailForge.Domain.Enums;

namespace MailForge.Application.DTOs.Verifier;

public record VerifyRequest(string Email);

public record VerifyResponse(
    string Email,
    EmailStatus Status,
    int Score,
    bool Mx,
    bool Smtp,
    bool CatchAll,
    bool Disposable,
    bool RoleBased,
    string? Details);
