
namespace CurrencyExchanger.API.Models.Responses;

public class ExchangeRateResponse
{
    public required int? Id { get; set; }
    public required CurrencyResponse BaseCurrency { get; set; }
    public required CurrencyResponse TargetCurrency { get; set; }
    public required decimal? Rate { get; set; }
}
