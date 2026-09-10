
namespace CurrencyExchanger.API;

public interface IProblemDetails
{
    int? Code { get; set; }
    string? Message { get; set; }
    string? Title { get; set; }
}
