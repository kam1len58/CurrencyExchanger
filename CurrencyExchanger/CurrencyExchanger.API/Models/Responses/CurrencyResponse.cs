
namespace CurrencyExchanger.API.Models.Responses;

public class CurrencyResponse
{
    public required int? Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Sign { get; set; }
}
