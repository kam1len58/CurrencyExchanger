
namespace CurrencyExchanger.Models;

public class Currency(int id, string code, string fullName, string sign)
{
    public int ID { get; } = id;
    public string Code { get; } = code;
    public string FullName { get; } = fullName;
    public string Sign { get; } = sign;
}
