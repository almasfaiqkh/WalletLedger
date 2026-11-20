using WalletLedger.Application.DTOs;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Exceptions;

namespace WalletLedger.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IWalletRepository walletRepository,
        ILedgerRepository ledgerRepository,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _ledgerRepository = ledgerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet pengguna tidak ditemukan.");

        return new WalletBalanceResponse(
            wallet.Id,
            wallet.User?.FullName ?? "Unknown",
            wallet.User?.Email ?? "Unknown",
            wallet.Balance,
            wallet.Currency,
            wallet.UpdatedAt
        );
    }

    public async Task<TransactionResponse> TopUpAsync(Guid userId, TopUpRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            throw new BusinessRuleException("Nominal top-up harus lebih besar dari 0.");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new BusinessRuleException("IdempotencyKey wajib diisi.");

        // 1. Cek Idempotency: Kembalikan transaksi lama jika key sudah pernah diproses
        var existingTx = await _ledgerRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existingTx is not null)
            return MapToResponse(existingTx);

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet pengguna tidak ditemukan.");

        // 2. Eksekusi mutasi dalam transaksi ACID
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new LedgerTransaction
            {
                ReferenceCode = GenerateReferenceCode(),
                IdempotencyKey = request.IdempotencyKey.Trim(),
                Type = TransactionType.TopUp,
                SourceWalletId = null,
                DestinationWalletId = wallet.Id,
                Amount = request.Amount,
                Description = string.IsNullOrWhiteSpace(request.Description) ? "Top-up saldo" : request.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _ledgerRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToResponse(transaction);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TransactionResponse> TransferAsync(Guid senderUserId, TransferRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            throw new BusinessRuleException("Nominal transfer harus lebih besar dari 0.");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new BusinessRuleException("IdempotencyKey wajib diisi.");

        // 1. Cek Idempotency terlebih dahulu
        var existingTx = await _ledgerRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existingTx is not null)
            return MapToResponse(existingTx);

        var senderWallet = await _walletRepository.GetByUserIdAsync(senderUserId, cancellationToken)
            ?? throw new NotFoundException("Wallet pengirim tidak ditemukan.");

        var recipientWallet = await _walletRepository.GetByUserEmailAsync(request.RecipientEmail.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException($"Akun penerima dengan email '{request.RecipientEmail}' tidak ditemukan.");

        if (senderWallet.Id == recipientWallet.Id)
            throw new BusinessRuleException("Tidak dapat melakukan transfer ke wallet sendiri.");

        if (senderWallet.Balance < request.Amount)
            throw new InsufficientBalanceException("Saldo tidak mencukupi untuk melakukan transfer.");

        // 2. Eksekusi debit-kredit secara atomik
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            senderWallet.Balance -= request.Amount;
            senderWallet.UpdatedAt = DateTime.UtcNow;

            recipientWallet.Balance += request.Amount;
            recipientWallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new LedgerTransaction
            {
                ReferenceCode = GenerateReferenceCode(),
                IdempotencyKey = request.IdempotencyKey.Trim(),
                Type = TransactionType.Transfer,
                SourceWalletId = senderWallet.Id,
                DestinationWalletId = recipientWallet.Id,
                Amount = request.Amount,
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? $"Transfer ke {recipientWallet.User?.FullName ?? request.RecipientEmail}"
                    : request.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _ledgerRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToResponse(transaction);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionResponse>> GetTransactionHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Wallet pengguna tidak ditemukan.");

        var transactions = await _ledgerRepository.GetHistoryByWalletIdAsync(wallet.Id, cancellationToken);
        return transactions.Select(MapToResponse);
    }

    private static string GenerateReferenceCode()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"TRX-{datePart}-{randomPart}";
    }

    private static TransactionResponse MapToResponse(LedgerTransaction tx) => new(
        tx.Id,
        tx.ReferenceCode,
        tx.IdempotencyKey,
        tx.Type,
        tx.SourceWalletId,
        tx.DestinationWalletId,
        tx.Amount,
        tx.Description,
        tx.CreatedAt
    );
}