using WalletLedger.Domain.Entities;

namespace WalletLedger.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}

public interface IAuthService
{
    Task<DTOs.AuthResponse> RegisterAsync(DTOs.RegisterRequest request, CancellationToken cancellationToken = default);
    Task<DTOs.AuthResponse> LoginAsync(DTOs.LoginRequest request, CancellationToken cancellationToken = default);
}