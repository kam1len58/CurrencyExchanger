
namespace CurrencyExchanger.API.Models.Requests;

public class CurrencyRequest
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Sign { get; set; }
}
