using CurrencyExchanger.DAL.Dao;

namespace CurrencyExchanger.API.Extensions;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(sp => new DBConnectionProvider(connectionString));
        services.AddScoped<CurrencyDao>();
        services.AddScoped<ExchangeRateDao>();

        return services;
    }
}
