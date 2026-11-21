using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WalletLedger.Application.Interfaces;
using WalletLedger.Application.Services;

namespace WalletLedger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}