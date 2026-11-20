using WalletLedger.Application.DTOs;

namespace WalletLedger.Application.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceResponse> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TransactionResponse> TopUpAsync(Guid userId, TopUpRequest request, CancellationToken cancellationToken = default);
    Task<TransactionResponse> TransferAsync(Guid senderUserId, TransferRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TransactionResponse>> GetTransactionHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}