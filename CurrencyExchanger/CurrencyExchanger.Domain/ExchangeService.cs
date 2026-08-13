using CurrencyExchanger.Domain.Validators;

namespace CurrencyExchanger.Domain;

public class ExchangeService
{
    private readonly List<Currency> _currencies;
    private readonly List<ExchangeRate> _exchangeRates;

    public ExchangeService(List<Currency> currencies, List<ExchangeRate> exchangeRates)
    {
        _currencies = currencies;
        _exchangeRates = exchangeRates;
    }

    public (decimal? Rate, decimal? ConvertedAmount) Exchange(int from, int to, decimal? amount)
    {
        var exchangeRateValidator = new ExchangeRateValidator();
        exchangeRateValidator.ValidateAmount(amount);
        var rate = GetDirectRate(from, to);
        if (rate is null)
            rate = GetInverseRate(from, to);

        if (rate is null)
            rate = GetCrossRate(from, to);

        if (amount is null)
            return (rate, null);

        if (rate is null)
            return (null, null);

        return (rate, Math.Round(rate.Value * amount.Value, 2, MidpointRounding.AwayFromZero));
    }

    private decimal? GetDirectRate(int currencyId, int targetId)
    {
        var exchangeRate = _exchangeRates.FirstOrDefault(er => er.TargetCurrencyId == targetId && er.BaseCurrencyId == currencyId);
        if (exchangeRate is null)
            return null;

        return exchangeRate.Rate;
    }

    private decimal? GetInverseRate(int currencyId, int targetId)
    {
        var exchangeRate = _exchangeRates.FirstOrDefault(er => er.BaseCurrencyId == targetId && er.TargetCurrencyId == currencyId);
        if (exchangeRate is null)
            return null;

        return 1 / exchangeRate.Rate;
    }

    private decimal? GetCrossRate(int currencyId, int targetId)
    {
        var usd = _currencies.FirstOrDefault(c => c.Code == "USD");
        if (usd is null)
            return null;

        var baseExchangeRate = _exchangeRates.FirstOrDefault(er => er.BaseCurrencyId == usd.ID && er.TargetCurrencyId == targetId);
        var targetExchangeRate = _exchangeRates.FirstOrDefault(er => er.TargetCurrencyId == currencyId && er.BaseCurrencyId == usd.ID);
        if (baseExchangeRate is null || targetExchangeRate is null)
            return null;

        return baseExchangeRate.Rate / targetExchangeRate.Rate;
    }
}
