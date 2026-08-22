
namespace CurrencyExchanger.Models.Dto;

public record ExchangeRateDto(int ID, CurrencyDto? BaseCurrency, CurrencyDto? TargetCurrency, decimal? Rate);

