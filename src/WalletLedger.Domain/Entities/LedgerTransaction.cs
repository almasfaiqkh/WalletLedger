using WalletLedger.Domain.Enums;

namespace WalletLedger.Domain.Entities;

public class LedgerTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReferenceCode { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public TransactionType Type { get; set; }

    // Boleh kosong (null) jika jenisnya TopUp dari luar sistem
    public Guid? SourceWalletId { get; set; }
    public Guid DestinationWalletId { get; set; }

    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}