using CurrencyExchanger.DAL.Dao;
using CurrencyExchanger.Models;

namespace CurrencyExchanger.BLL.Services;

public class CurrencyService(CurrencyDao currencyDao)
{
    private readonly CurrencyDao _currencyDao = currencyDao;

    public async Task<bool> IsCurrencyExistsAsync(string code) => await _currencyDao.IsCurrencyExistsAsync(code);

    public async Task<List<Currency>> GetAllCurrenciesAsync() => await _currencyDao.GetAllCurrenciesAsync();

    public async Task<Currency> AddCurrencyAsync(string name, string code, string sign) => await _currencyDao.AddCurrencyAsync(name, code, sign);

    public async Task<Currency?> GetCurrencyByCodeAsync(string code) => await _currencyDao.GetCurrencyByCodeAsync(code);

    public async Task<int?> GetCurrencyIdByCodeAsync(string code) => await _currencyDao.GetCurrencyIdByCodeAsync(code);

    public async Task<Currency?> GetCurrencyByIdAsync(int? id) => await _currencyDao.GetCurrencyByIdAsync(id);
}
