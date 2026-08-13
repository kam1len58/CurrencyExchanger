
namespace CurrencyExchanger.Domain;

public class ExchangeRate
{
    public ExchangeRate(int id, int baseCurrencyId, int targetCurrencyId, decimal? rate)
    {
        ID = id;
        BaseCurrencyId = baseCurrencyId;
        TargetCurrencyId = targetCurrencyId;
        Rate = rate;
    }

    public int ID { get; }
    public int BaseCurrencyId { get; }
    public int TargetCurrencyId { get; }
    public decimal? Rate { get; }
}
