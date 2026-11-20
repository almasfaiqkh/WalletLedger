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
    public uint RowVersion { get; set; }

    public User? User { get; set; }
}