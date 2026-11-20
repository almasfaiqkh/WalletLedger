using System.ComponentModel.DataAnnotations;

namespace WalletLedger.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0m;
    public string Currency { get; set; } = "IDR";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Mencegah race condition saat saldo diubah bersamaan
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public User? User { get; set; }
}