using CurrencyExchanger.Models;
using CurrencyExchanger.Models.Dto;

namespace CurrencyExchanger.DAL.Mappers;

public static class ExchangeMapper
{
    public static ExchangeDto ToDto(Currency baseCurrency, Currency targetCurrency, decimal? rate, decimal? amount, decimal? convertedAmount) =>
        new ExchangeDto(
            CurrencyMapper.ToDto(baseCurrency),
            CurrencyMapper.ToDto(targetCurrency),
            rate,
            amount,
            convertedAmount
        );
}
