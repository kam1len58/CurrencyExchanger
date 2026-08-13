using CurrencyExchanger.Domain;
using CurrencyExchanger.Dto;

namespace CurrencyExchanger.Mappers;

public static class ExchangeMapper
{
    public static ExchangeDto ToDto(Currency baseCurrency, Currency targetCurrency, decimal? rate, decimal? amount, decimal? convertedAmount) => 
        new ExchangeDto(CurrencyMapper.ToDto(baseCurrency), CurrencyMapper.ToDto(targetCurrency), rate, amount, convertedAmount);
}
