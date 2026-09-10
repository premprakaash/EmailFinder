namespace MailForge.Application.Interfaces;

public record CompanyResolveResult(string CompanyName, string Domain, string Source);

public interface ICompanyResolver
{
    Task<CompanyResolveResult?> ResolveAsync(string query, CancellationToken cancellationToken = default);
}
