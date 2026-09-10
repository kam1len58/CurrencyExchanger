using CurrencyExchanger.DAL.Dao;

namespace CurrencyExchanger.BLL.Services;

public class ExchangeService(CurrencyDao currencyDao, ExchangeRateDao exchangeRateDao)
{
    public async Task<(decimal? Rate, decimal? ConvertedAmount)> ExchangeAsync(int from, int to, decimal? amount)
    {
        if (amount is null || amount <= 0)
            throw new ArgumentException("Неверная сумма при конвертации валют");

        var rate = (await GetDirectRateAsync(from, to))
            ?? (await GetInverseRateAsync(from, to))
            ?? (await GetCrossRateAsync(from, to));

        if (rate is null)
            return (null, null);

        return (rate, Math.Round(rate.Value * amount.Value, 2, MidpointRounding.AwayFromZero));
    }

    private async Task<decimal?> GetDirectRateAsync(int currencyId, int targetId)
    {
        var exchangeRates = await exchangeRateDao.GetAllExchangeRatesAsync();

        return exchangeRates.FirstOrDefault(er => er.TargetCurrencyId == targetId && er.BaseCurrencyId == currencyId)?.Rate;
    }



    private async Task<decimal?> GetInverseRateAsync(int currencyId, int targetId)
    {
        var exchangeRates = await exchangeRateDao.GetAllExchangeRatesAsync();
        var exchangeRate = exchangeRates.FirstOrDefault(er => er.BaseCurrencyId == targetId && er.TargetCurrencyId == currencyId);

        return exchangeRate?.Rate is null ? null : 1 / exchangeRate.Rate;
    }

    private async Task<decimal?> GetCrossRateAsync(int currencyId, int targetId)
    {
        var exchangeRates = await exchangeRateDao.GetAllExchangeRatesAsync();
        var currencies = await currencyDao.GetAllCurrenciesAsync();
        var usd = currencies.FirstOrDefault(c => c.Code == "USD");

        if (usd is null)
            return null;

        var baseExchangeRate = exchangeRates.FirstOrDefault(er => er.BaseCurrencyId == usd.Id && er.TargetCurrencyId == targetId);
        var targetExchangeRate = exchangeRates.FirstOrDefault(er => er.TargetCurrencyId == currencyId && er.BaseCurrencyId == usd.Id);

        if (baseExchangeRate is null || targetExchangeRate is null)
            return null;

        return baseExchangeRate.Rate / targetExchangeRate.Rate;
    }
}
