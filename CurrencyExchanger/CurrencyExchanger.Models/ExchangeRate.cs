
namespace CurrencyExchanger.Models;

public class ExchangeRate(int id, int? baseCurrencyId, int? targetCurrencyId, decimal? rate)
{
    public int ID { get; } = id;
    public int? BaseCurrencyId { get; } = baseCurrencyId;
    public int? TargetCurrencyId { get; } = targetCurrencyId;
    public decimal? Rate { get; } = rate;
}
