using CurrencyExchanger.Models;
using CurrencyExchanger.Models.Dto;

namespace CurrencyExchanger.DAL.Mappers;

public static class ExchangeRateMapper
{
    public static ExchangeRateDto ToDto(ExchangeRate exchangeRate, Currency baseCurrency, Currency targetCurrency) =>
        new ExchangeRateDto(
            exchangeRate.ID,
            CurrencyMapper.ToDto(baseCurrency),
            CurrencyMapper.ToDto(targetCurrency),
            exchangeRate.Rate
        );
}
