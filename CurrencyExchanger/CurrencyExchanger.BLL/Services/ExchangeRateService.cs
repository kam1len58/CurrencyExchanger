using CurrencyExchanger.DAL.Dao;
using CurrencyExchanger.Models;

namespace CurrencyExchanger.BLL.Services;

public class ExchangeRateService(ExchangeRateDao exchangeRateDao)
{
    private readonly ExchangeRateDao _exchangeRateDao = exchangeRateDao;

    public async Task<(ExchangeRate ExchangeRate, Currency BaseCurrency, Currency TargetCurrency)?> GetExchangeRateByCodeAsync(string pair) =>
        await _exchangeRateDao.GetExchangeRateByCodeAsync(pair);

    public async Task<bool> IsExchangeRateExistsAsync(int? baseCurrencyId, int? targetCurrencyId) =>
        await _exchangeRateDao.IsExchangeRateExistsAsync(baseCurrencyId, targetCurrencyId);

    public async Task<(ExchangeRate ExchangeRate, Currency BaseCurrency, Currency TargetCurrency)?> UpdateExchangeRateDtoAsync(string pair, decimal? rate) =>
        await _exchangeRateDao.UpdateExchangeRateDtoAsync(pair, rate);

    public async Task<List<ExchangeRate>> GetAllExchangeRatesAsync() => await _exchangeRateDao.GetAllExchangeRatesAsync();

    public async Task<(ExchangeRate ExchangeRate, Currency BaseCurrency, Currency TargetCurrency)?> AddExchangeRateAsync(int? baseCurrencyId, int? targetCurrencyId, decimal? rate) =>
        await _exchangeRateDao.AddExchangeRateAsync(baseCurrencyId, targetCurrencyId, rate);
}
