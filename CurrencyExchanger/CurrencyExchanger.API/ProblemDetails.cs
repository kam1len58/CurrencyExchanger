
namespace CurrencyExchanger.API;

public class ProblemDetails : IProblemDetails
{
    public int? Code { get; set; }
    public string? Message { get; set; }
    public string? Title { get; set; }
}
