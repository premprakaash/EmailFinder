using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Services;

public class CreditService : ICreditService
{
    private readonly MailForgeDbContext _db;
    private readonly ILogger<CreditService> _logger;

    public CreditService(MailForgeDbContext db, ILogger<CreditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<long> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var balance = await _db.CreditBalances.AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);
        return balance?.Balance ?? 0;
    }

    public async Task<long> GetCreditCostAsync(CreditOperation operation, CancellationToken cancellationToken)
    {
        var rule = await _db.CreditRules.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Operation == operation && r.IsActive, cancellationToken);
        return rule?.CreditCost ?? 1;
    }

    public async Task<bool> ConsumeCreditsAsync(Guid userId, CreditOperation operation, string description, string? referenceId = null, CancellationToken cancellationToken = default)
    {
        var cost = await GetCreditCostAsync(operation, cancellationToken);
        if (cost <= 0) return true;

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var balance = await _db.CreditBalances
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (balance == null)
            {
                balance = new CreditBalance { UserId = userId, Balance = 0 };
                _db.CreditBalances.Add(balance);
                await _db.SaveChangesAsync(cancellationToken);
            }

            if (balance.Balance < cost)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            balance.Balance -= cost;
            balance.TotalConsumed += cost;

            _db.CreditTransactions.Add(new CreditTransaction
            {
                UserId = userId,
                Operation = operation,
                Amount = -cost,
                BalanceAfter = balance.Balance,
                Description = description,
                ReferenceId = referenceId
            });

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to consume credits for user {UserId}", userId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task AddCreditsAsync(Guid userId, long amount, CreditOperation operation, string description, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var balance = await _db.CreditBalances
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (balance == null)
            {
                balance = new CreditBalance { UserId = userId };
                _db.CreditBalances.Add(balance);
            }

            balance.Balance += amount;
            balance.TotalPurchased += amount;

            _db.CreditTransactions.Add(new CreditTransaction
            {
                UserId = userId,
                Operation = operation,
                Amount = amount,
                BalanceAfter = balance.Balance,
                Description = description
            });

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add credits for user {UserId}", userId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
