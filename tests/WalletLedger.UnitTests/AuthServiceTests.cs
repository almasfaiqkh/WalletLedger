using Moq;
using WalletLedger.Application.DTOs;
using WalletLedger.Application.Interfaces;
using WalletLedger.Application.Services;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Exceptions;

namespace WalletLedger.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepoMock.Object,
            _walletRepoMock.Object,
            _hasherMock.Object,
            _jwtMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndWalletAndReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest("Almas Fa'iq", "almas@mail.com", "Password123!");
        var expiry = DateTime.UtcNow.AddHours(1);

        _userRepoMock.Setup(r => r.ExistsByEmailAsync("almas@mail.com", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("Password123!")).Returns("hashed_password");
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("mock_jwt_token", expiry));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.Equal("almas@mail.com", result.Email);
        Assert.Equal("mock_jwt_token", result.Token);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _walletRepoMock.Verify(r => r.AddAsync(It.Is<Wallet>(w => w.Balance == 0m), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsBusinessRuleException()
    {
        // Arrange
        var user = new User { Email = "almas@mail.com", PasswordHash = "hashed_password" };
        var request = new LoginRequest("almas@mail.com", "WrongPassword!");

        _userRepoMock.Setup(r => r.GetByEmailAsync("almas@mail.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("WrongPassword!", "hashed_password")).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.LoginAsync(request));
    }
}