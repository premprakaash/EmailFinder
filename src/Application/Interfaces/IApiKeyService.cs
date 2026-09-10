namespace MailForge.Application.Interfaces;

public interface IApiKeyService
{
    Task<(string RawKey, Guid Id)> CreateKeyAsync(Guid userId, string name, CancellationToken cancellationToken);
    Task RevokeKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken);
    Task<Guid?> ValidateKeyAsync(string rawKey, CancellationToken cancellationToken);
    Task RecordUsageAsync(Guid keyId, CancellationToken cancellationToken);
}
