
namespace CurrencyExchanger.Models;

public record ExchangeRate(int Id, int? BaseCurrencyId, int? TargetCurrencyId, decimal? Rate);

