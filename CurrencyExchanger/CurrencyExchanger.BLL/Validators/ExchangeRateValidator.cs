using System.Text.RegularExpressions;

namespace CurrencyExchanger.BLL.Validators;

public class ExchangeRateValidator
{
    public void ValidateParameters(string baseCurrencyCode, string targetCurrencyCode, decimal? rate)
    {
        ValidateCode(baseCurrencyCode, targetCurrencyCode);
        ValidateRate(rate);
    }

    private void ValidateCode(string baseCurrencyCode, string targetCurrencyCode)
    {
        if (string.IsNullOrEmpty(baseCurrencyCode)
            || string.IsNullOrEmpty(targetCurrencyCode)
            || !Regex.IsMatch(baseCurrencyCode, @"^[A-Z]{3}$")
            || !Regex.IsMatch(targetCurrencyCode, @"^[A-Z]{3}$"))
        {
            throw new ArgumentException("Некорректный код при добавлении обменного курса валют");
        }
    }

    private void ValidateRate(decimal? rate)
    {
        if (rate is null || rate <= 0)
        {
            throw new ArgumentException("Некорректный курс при добавлении в базу данных");
        }
    }
}
