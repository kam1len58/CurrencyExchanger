
namespace CurrencyExchanger.API.Models.Requests;

public class ExchangeRateRequest
{
    public required string BaseCurrencyCode { get; set; }
    public required string TargetCurrencyCode { get; set; }
    public required decimal? Rate { get; set; }
}
