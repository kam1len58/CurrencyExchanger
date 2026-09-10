
namespace CurrencyExchanger.API.Models.Responses;

public class ExchangeResponse
{
    public required CurrencyResponse BaseCurrency { get; set; }
    public required CurrencyResponse TargetCurrency { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? ConvertedAmount { get; set; }
}
