using WalletLedger.Domain.Entities;

namespace WalletLedger.Application.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wallet?> GetByUserEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
}

public interface ILedgerRepository
{
    Task<LedgerTransaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerTransaction>> GetHistoryByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}