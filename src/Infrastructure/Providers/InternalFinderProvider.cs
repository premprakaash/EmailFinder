using MailForge.Application.DTOs.Finder;
using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;
using MailForge.Domain.Enums;

namespace MailForge.Infrastructure.Providers;

public class InternalFinderProvider : IEmailFinderProvider
{
    private readonly IEmailPatternEngine _patternEngine;
    private readonly IEnumerable<IEmailVerificationProvider> _verificationProviders;

    public string Name => "Internal";
    public int Priority => 100;

    public InternalFinderProvider(IEmailPatternEngine patternEngine, IEnumerable<IEmailVerificationProvider> verificationProviders)
    {
        _patternEngine = patternEngine;
        _verificationProviders = verificationProviders.OrderBy(p => p.Priority);
    }

    public async Task<IReadOnlyList<ProviderFinderResult>> FindAsync(ProviderFinderRequest request, CancellationToken cancellationToken)
    {
        var patterns = _patternEngine.GeneratePatterns(request.FirstName, request.LastName, request.Domain);
        var results = new List<ProviderFinderResult>();

        foreach (var email in patterns)
        {
            var pattern = email.Split('@')[0];
            var confidence = 50;
            var verified = false;

            foreach (var verifier in _verificationProviders)
            {
                try
                {
                    var vr = await verifier.VerifyAsync(email, cancellationToken);
                    confidence = vr.Score;
                    verified = vr.Status == EmailStatus.Valid;
                    break;
                }
                catch { /* continue */ }
            }

            results.Add(new ProviderFinderResult(email, confidence, pattern, Name, verified));
        }

        return results.OrderByDescending(r => r.ConfidenceScore).ToList();
    }
}
