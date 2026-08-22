using CurrencyExchanger.Models;
using CurrencyExchanger.Models.Dto;

namespace CurrencyExchanger.DAL.Mappers;

public static class CurrencyMapper
{
    public static CurrencyDto ToDto(Currency currency) =>
        new CurrencyDto(
            currency.ID,
            currency.Code,
            currency.FullName,
            currency.Sign
        );
}


