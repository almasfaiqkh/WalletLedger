using Microsoft.EntityFrameworkCore;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;

namespace WalletLedger.Infrastructure.Persistence.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly AppDbContext _context;

    public LedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerTransaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<IEnumerable<LedgerTransaction>> GetHistoryByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .Where(t => t.SourceWalletId == walletId || t.DestinationWalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.LedgerTransactions.AddAsync(transaction, cancellationToken);
    }
}