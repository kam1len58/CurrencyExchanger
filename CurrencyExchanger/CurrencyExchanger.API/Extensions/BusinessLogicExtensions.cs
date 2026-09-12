using CurrencyExchanger.BLL.Services;
using CurrencyExchanger.BLL.Validators;

namespace CurrencyExchanger.API.Extensions;

public static class BusinessLogicExtensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddSingleton<CurrencyValidator>();
        services.AddSingleton<ExchangeRateValidator>();
        services.AddScoped<ExchangeService>();
        services.AddScoped<CurrencyService>();
        services.AddScoped<ExchangeRateService>();

        return services;
    }
}
