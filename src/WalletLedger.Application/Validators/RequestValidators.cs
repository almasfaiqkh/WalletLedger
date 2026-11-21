using FluentValidation;
using WalletLedger.Application.DTOs;

namespace WalletLedger.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nama lengkap wajib diisi.")
            .MaximumLength(100).WithMessage("Nama lengkap maksimal 100 karakter.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.")
            .MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.")
            .MinimumLength(8).WithMessage("Password minimal 8 karakter.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.");
    }
}

public class TopUpRequestValidator : AbstractValidator<TopUpRequest>
{
    public TopUpRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal top-up harus lebih besar dari 0.")
            .LessThanOrEqualTo(100_000_000m).WithMessage("Maksimal top-up Rp 100.000.000 per transaksi.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey wajib diisi.")
            .MaximumLength(100);
    }
}

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("Email penerima wajib diisi.")
            .EmailAddress().WithMessage("Format email penerima tidak valid.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Nominal transfer harus lebih besar dari 0.")
            .LessThanOrEqualTo(50_000_000m).WithMessage("Maksimal transfer Rp 50.000.000 per transaksi.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey wajib diisi.")
            .MaximumLength(100);
    }
}