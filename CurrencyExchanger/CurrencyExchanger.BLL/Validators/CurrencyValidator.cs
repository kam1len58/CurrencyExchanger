using System.Text.RegularExpressions;

namespace CurrencyExchanger.BLL.Validators;

public class CurrencyValidator
{
    public void ValidateCurrencyParameters(string code, string name, string sign)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateSign(sign);
    }

    private void ValidateCode(string code)
    {
        if (string.IsNullOrEmpty(code)
            || !Regex.IsMatch(code, @"^[A-Z]{3}$"))
            throw new ArgumentException("Некорректный код валюты при добавлении в базу данных");
    }

    private void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name)
            || !Regex.IsMatch(name, @"^[A-Za-z\s\-]{2,50}$"))
            throw new ArgumentException("Некорректное название валюты при добавлении в базу данных");
    }

    private void ValidateSign(string sign)
    {
        if (string.IsNullOrEmpty(sign)
            || !Regex.IsMatch(sign, @"\S"))
            throw new ArgumentException("Некорректный знак валюты при добавлении в базу данных");
    }
}
