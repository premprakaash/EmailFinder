using MailForge.Domain.Enums;

namespace MailForge.Application.Interfaces;

public interface ICreditService
{
    Task<long> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);
    Task<long> GetCreditCostAsync(CreditOperation operation, CancellationToken cancellationToken);
    Task<bool> ConsumeCreditsAsync(Guid userId, CreditOperation operation, string description, string? referenceId = null, CancellationToken cancellationToken = default);
    Task AddCreditsAsync(Guid userId, long amount, CreditOperation operation, string description, CancellationToken cancellationToken = default);
}
