using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;
using MailForge.Application.DTOs.Verifier;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Services;

public class VerifierService : IVerifierService
{
    private readonly IEnumerable<IEmailVerificationProvider> _providers;
    private readonly ICreditService _creditService;
    private readonly MailForgeDbContext _db;
    private readonly ILogger<VerifierService> _logger;

    public VerifierService(
        IEnumerable<IEmailVerificationProvider> providers,
        ICreditService creditService,
        MailForgeDbContext db,
        ILogger<VerifierService> logger)
    {
        _providers = providers.OrderBy(p => p.Priority);
        _creditService = creditService;
        _db = db;
        _logger = logger;
    }

    public async Task<VerifyResponse> VerifyAsync(Guid userId, VerifyRequest request, CancellationToken cancellationToken)
    {
        if (!await _creditService.ConsumeCreditsAsync(userId, CreditOperation.EmailVerification, "Email verification", cancellationToken: cancellationToken))
            throw new InvalidOperationException("Insufficient credits.");

        var email = request.Email.Trim().ToLowerInvariant();
        ProviderVerificationResult? result = null;

        foreach (var provider in _providers)
        {
            try
            {
                result = await provider.VerifyAsync(email, cancellationToken);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Verification provider {Provider} failed", provider.Name);
            }
        }

        result ??= new ProviderVerificationResult(email, EmailStatus.Unknown, 0, false, false, false, false, false, "None", "All providers failed");

        _db.VerificationResults.Add(new VerificationResult
        {
            UserId = userId,
            Email = email,
            Status = result.Status,
            Score = result.Score,
            MxValid = result.MxValid,
            SmtpValid = result.SmtpValid,
            CatchAll = result.CatchAll,
            Disposable = result.Disposable,
            RoleBased = result.RoleBased,
            Provider = result.ProviderName,
            Details = result.Details
        });

        _db.VerificationJobs.Add(new VerificationJob
        {
            UserId = userId,
            Email = email,
            Status = JobStatus.Completed,
            ResultStatus = result.Status,
            Score = result.Score
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new VerifyResponse(
            email, result.Status, result.Score,
            result.MxValid, result.SmtpValid, result.CatchAll,
            result.Disposable, result.RoleBased, result.Details);
    }
}
