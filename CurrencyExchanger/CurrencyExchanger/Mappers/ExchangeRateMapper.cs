using CurrencyExchanger.Domain;
using CurrencyExchanger.Dto;

namespace CurrencyExchanger.Mappers;

public static class ExchangeRateMapper
{
    public static ExchangeRateDto ToDto(ExchangeRate exchangeRate, Currency baseCurrency, Currency targetCurrency) => 
        new ExchangeRateDto(exchangeRate.ID, CurrencyMapper.ToDto(baseCurrency), CurrencyMapper.ToDto(targetCurrency), exchangeRate.Rate);
}
