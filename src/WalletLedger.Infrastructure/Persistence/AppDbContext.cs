using Microsoft.EntityFrameworkCore;
using WalletLedger.Domain.Entities;

namespace WalletLedger.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurasi Tabel User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();

            // Relasi 1-ke-1 antara User dan Wallet
            entity.HasOne(u => u.Wallet)
                  .WithOne(w => w.User)
                  .HasForeignKey<Wallet>(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel Wallet
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Balance).HasPrecision(18, 2);
            entity.Property(w => w.Currency).IsRequired().HasMaxLength(3);
            entity.HasIndex(w => w.UserId).IsUnique();

            // Menggunakan kolom sistem xmin PostgreSQL sebagai concurrency token
            entity.Property(w => w.RowVersion).IsRowVersion();
        });

        // Konfigurasi Tabel LedgerTransaction
        modelBuilder.Entity<LedgerTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.ReferenceCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(t => t.ReferenceCode).IsUnique();

            entity.Property(t => t.IdempotencyKey).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.IdempotencyKey).IsUnique();

            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Description).HasMaxLength(255);

            // Relasi ke SourceWallet (opsional untuk TopUp) & DestinationWallet
            entity.HasOne<Wallet>()
                  .WithMany()
                  .HasForeignKey(t => t.SourceWalletId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Wallet>()
                  .WithMany()
                  .HasForeignKey(t => t.DestinationWalletId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}