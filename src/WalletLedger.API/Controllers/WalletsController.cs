using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletLedger.Application.DTOs;
using WalletLedger.Application.Interfaces;

namespace WalletLedger.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IValidator<TopUpRequest> _topUpValidator;
    private readonly IValidator<TransferRequest> _transferValidator;

    public WalletsController(
        IWalletService walletService,
        IValidator<TopUpRequest> topUpValidator,
        IValidator<TransferRequest> transferValidator)
    {
        _walletService = walletService;
        _topUpValidator = topUpValidator;
        _transferValidator = transferValidator;
    }

    [HttpGet("me")]
    public async Task<ActionResult<WalletBalanceResponse>> GetMyWallet(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var balance = await _walletService.GetBalanceAsync(userId, cancellationToken);
        return Ok(balance);
    }

    [HttpPost("top-up")]
    public async Task<ActionResult<TransactionResponse>> TopUp(
        [FromBody] TopUpRequest request,
        CancellationToken cancellationToken)
    {
        await _topUpValidator.ValidateAndThrowAsync(request, cancellationToken);
        var userId = GetCurrentUserId();
        var transaction = await _walletService.TopUpAsync(userId, request, cancellationToken);
        return Ok(transaction);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<TransactionResponse>> Transfer(
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken)
    {
        await _transferValidator.ValidateAndThrowAsync(request, cancellationToken);
        var userId = GetCurrentUserId();
        var transaction = await _walletService.TransferAsync(userId, request, cancellationToken);
        return Ok(transaction);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactionHistory(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var history = await _walletService.GetTransactionHistoryAsync(userId, cancellationToken);
        return Ok(history);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Token pengguna tidak valid.");

        return userId;
    }
}