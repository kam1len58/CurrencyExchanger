using CurrencyExchanger.Domain;
using CurrencyExchanger.Dto;

namespace CurrencyExchanger.Mappers;

public static class CurrencyMapper
{
    public static CurrencyDto ToDto(Currency currency) => new CurrencyDto(currency.ID, currency.Code, currency.FullName, currency.Sign);
}


