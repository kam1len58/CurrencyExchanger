
namespace CurrencyExchanger.Domain;

public class Currency
{
    public Currency(int id, string code, string fullName, string sign)
    {
        ID = id;
        Code = code;
        FullName = fullName;
        Sign = sign;
    }

    public int ID { get; }
    public string Code { get; }
    public string FullName { get; }
    public string Sign { get; }
}
