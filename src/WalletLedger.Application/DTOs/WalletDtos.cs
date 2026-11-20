using WalletLedger.Domain.Enums;

namespace WalletLedger.Application.DTOs;

public record TopUpRequest(
    decimal Amount,
    string IdempotencyKey,
    string? Description
);

public record TransferRequest(
    string RecipientEmail,
    decimal Amount,
    string IdempotencyKey,
    string? Description
);

public record WalletBalanceResponse(
    Guid WalletId,
    string OwnerName,
    string Email,
    decimal Balance,
    string Currency,
    DateTime UpdatedAt
);

public record TransactionResponse(
    Guid TransactionId,
    string ReferenceCode,
    string IdempotencyKey,
    TransactionType Type,
    Guid? SourceWalletId,
    Guid DestinationWalletId,
    decimal Amount,
    string Description,
    DateTime CreatedAt
);