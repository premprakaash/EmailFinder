using MailForge.Application.DTOs.Verifier;

namespace MailForge.Application.Interfaces;

public interface IVerifierService
{
    Task<VerifyResponse> VerifyAsync(Guid userId, VerifyRequest request, CancellationToken cancellationToken);
}
