
namespace CurrencyExchanger.API.Models.Requests;

public class ExchangeRequest
{
    public required string From { get; set; }
    public required string To { get; set; }
    public required decimal? Amount { get; set; }
}
