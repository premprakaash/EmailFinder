using MailForge.Domain.Enums;

namespace MailForge.Application.Interfaces.Providers;

public record ProviderVerificationResult(
    string Email,
    EmailStatus Status,
    int Score,
    bool MxValid,
    bool SmtpValid,
    bool CatchAll,
    bool Disposable,
    bool RoleBased,
    string ProviderName,
    string? Details = null);

public interface IEmailVerificationProvider
{
    string Name { get; }
    int Priority { get; }
    Task<ProviderVerificationResult> VerifyAsync(string email, CancellationToken cancellationToken);
}
