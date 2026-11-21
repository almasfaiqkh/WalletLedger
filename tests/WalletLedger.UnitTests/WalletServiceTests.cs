using Moq;
using WalletLedger.Application.DTOs;
using WalletLedger.Application.Interfaces;
using WalletLedger.Application.Services;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Exceptions;

namespace WalletLedger.UnitTests;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<ILedgerRepository> _ledgerRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly WalletService _sut;

    public WalletServiceTests()
    {
        _sut = new WalletService(
            _walletRepoMock.Object,
            _ledgerRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task TopUpAsync_WithValidRequest_IncreasesBalanceAndCommitsTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 100_000m };
        var request = new TopUpRequest(250_000m, "idem-topup-1", "Gaji bulanan");

        _ledgerRepoMock.Setup(r => r.GetByIdempotencyKeyAsync(request.IdempotencyKey, default))
            .ReturnsAsync((LedgerTransaction?)null);
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(wallet);

        // Act
        var result = await _sut.TopUpAsync(userId, request);

        // Assert
        Assert.Equal(350_000m, wallet.Balance);
        Assert.Equal(TransactionType.TopUp, result.Type);
        Assert.Equal(250_000m, result.Amount);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(default), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task TopUpAsync_WhenIdempotencyKeyAlreadyExists_ReturnsExistingTransactionWithoutModifyingBalance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingTx = new LedgerTransaction
        {
            Id = Guid.NewGuid(),
            ReferenceCode = "TRX-20261121-ABC123",
            IdempotencyKey = "idem-duplicate",
            Type = TransactionType.TopUp,
            DestinationWalletId = Guid.NewGuid(),
            Amount = 500_000m,
            Description = "Top-up awal"
        };
        var request = new TopUpRequest(500_000m, "idem-duplicate", "Top-up ulang");

        _ledgerRepoMock.Setup(r => r.GetByIdempotencyKeyAsync("idem-duplicate", default))
            .ReturnsAsync(existingTx);

        // Act
        var result = await _sut.TopUpAsync(userId, request);

        // Assert
        Assert.Equal(existingTx.Id, result.TransactionId);
        Assert.Equal(existingTx.ReferenceCode, result.ReferenceCode);

        // Pastikan database tidak disentuh ulang
        _walletRepoMock.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>(), default), Times.Never);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(default), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_WithSufficientBalance_TransfersFundsAtomically()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var senderWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = senderId,
            Balance = 500_000m,
            User = new User { FullName = "Alice", Email = "alice@mail.com" }
        };

        var recipientWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Balance = 100_000m,
            User = new User { FullName = "hiyuki", Email = "hiyuki@mail.com" }
        };

        var request = new TransferRequest("hiyuki@mail.com", 200_000m, "idem-tf-1", "Bayar makan");

        _ledgerRepoMock.Setup(r => r.GetByIdempotencyKeyAsync(request.IdempotencyKey, default))
            .ReturnsAsync((LedgerTransaction?)null);
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(senderId, default))
            .ReturnsAsync(senderWallet);
        _walletRepoMock.Setup(r => r.GetByUserEmailAsync("hiyuki@mail.com", default))
            .ReturnsAsync(recipientWallet);

        // Act
        var result = await _sut.TransferAsync(senderId, request);

        // Assert
        Assert.Equal(300_000m, senderWallet.Balance);
        Assert.Equal(300_000m, recipientWallet.Balance);
        Assert.Equal(TransactionType.Transfer, result.Type);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task TransferAsync_WhenBalanceIsInsufficient_ThrowsInsufficientBalanceException()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var senderWallet = new Wallet { Id = Guid.NewGuid(), UserId = senderId, Balance = 50_000m };
        var recipientWallet = new Wallet { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Balance = 0m };
        var request = new TransferRequest("hiyuki@mail.com", 100_000m, "idem-tf-fail", null);

        _ledgerRepoMock.Setup(r => r.GetByIdempotencyKeyAsync(request.IdempotencyKey, default))
            .ReturnsAsync((LedgerTransaction?)null);
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(senderId, default))
            .ReturnsAsync(senderWallet);
        _walletRepoMock.Setup(r => r.GetByUserEmailAsync("hiyuki@mail.com", default))
            .ReturnsAsync(recipientWallet);

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientBalanceException>(() => _sut.TransferAsync(senderId, request));
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(default), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ToOwnWallet_ThrowsBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 500_000m,
            User = new User { Email = "alice@mail.com" }
        };
        var request = new TransferRequest("alice@mail.com", 50_000m, "idem-self", null);

        _ledgerRepoMock.Setup(r => r.GetByIdempotencyKeyAsync(request.IdempotencyKey, default))
            .ReturnsAsync((LedgerTransaction?)null);
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(wallet);
        _walletRepoMock.Setup(r => r.GetByUserEmailAsync("alice@mail.com", default))
            .ReturnsAsync(wallet);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.TransferAsync(userId, request));
    }
}