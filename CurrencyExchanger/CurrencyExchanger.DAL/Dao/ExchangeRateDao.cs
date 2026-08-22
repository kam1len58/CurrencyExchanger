using CurrencyExchanger.DAL.Mappers;
using CurrencyExchanger.Models;
using CurrencyExchanger.Models.Dto;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.DAL.Dao;

public class ExchangeRateDao(DBConnectionProvider connectionProvider, CurrencyDao currencyDao)
{
    private readonly DBConnectionProvider _connectionProvider = connectionProvider;
    private readonly CurrencyDao _currencyDao = currencyDao;

    public async Task<List<ExchangeRate>> GetAllExchangeRatesAsync()
    {
        var exchangeRates = new List<ExchangeRate>();
        using var connection = _connectionProvider.GetConnection();

        await connection.OpenAsync();

        using var command = new SqliteCommand("SELECT * FROM ExchangeRates", connection);
        using var dataReader = await command.ExecuteReaderAsync();

        while (await dataReader.ReadAsync())
        {
            var exchangeRate = new ExchangeRate(
                dataReader.GetInt32(0),
                dataReader.GetInt32(1),
                dataReader.GetInt32(2),
                dataReader.GetDecimal(3)
            );

            exchangeRates.Add(exchangeRate);
        }

        return exchangeRates;
    }

    public async Task<ExchangeRateDto?> GetExchangeRateByCodeAsync(string pair)
    {
        string baseCurrencyCode = pair.Substring(0, 3);
        string targetCurrencyCode = pair.Substring(3, 3);

        var baseCurrency = await _currencyDao.GetCurrencyByCodeAsync(baseCurrencyCode);
        var targetCurrency = await _currencyDao.GetCurrencyByCodeAsync(targetCurrencyCode);

        if (baseCurrency is null || targetCurrency is null)
        {
            return null;
        }

        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        using var command = new SqliteCommand("SELECT * FROM ExchangeRates " +
            "JOIN Currencies AS BaseCurrency ON ExchangeRates.BaseCurrencyId = BaseCurrency.ID " +
            "JOIN Currencies AS TargetCurrency ON ExchangeRates.TargetCurrencyId = TargetCurrency.ID " +
            "WHERE BaseCurrency.Code = @baseCurrencyCode AND TargetCurrency.Code = @targetCurrencyCode", connection);

        command.Parameters.AddWithValue("@baseCurrencyCode", baseCurrencyCode);
        command.Parameters.AddWithValue("@targetCurrencyCode", targetCurrencyCode);

        using var dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            return ExchangeRateMapper.ToDto(
                new ExchangeRate(
                    dataReader.GetInt32(0),
                    baseCurrency.ID,
                    targetCurrency.ID,
                    dataReader.GetDecimal(3)
                ),
                baseCurrency,
                targetCurrency
            );
        }

        return null;
    }

    public async Task<ExchangeRateDto?> AddExchangeRateAsync(int? baseCurrencyId, int? targetCurrencyId, decimal? rate)
    {
        var baseCurrency = await _currencyDao.GetCurrencyByIdAsync(baseCurrencyId);
        var targetCurrency = await _currencyDao.GetCurrencyByIdAsync(targetCurrencyId);

        if (baseCurrency is null || targetCurrency is null)
        {
            return null;
        }

        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        using var command = new SqliteCommand("INSERT INTO ExchangeRates (BaseCurrencyId, TargetCurrencyId, Rate) VALUES (@baseCurrencyId, @targetCurrencyId, @rate);" +
            "SELECT last_insert_rowid();", connection);

        command.Parameters.AddWithValue("@baseCurrencyId", baseCurrencyId);
        command.Parameters.AddWithValue("@targetCurrencyId", targetCurrencyId);
        command.Parameters.AddWithValue("@rate", rate);

        int lastId = Convert.ToInt32(await command.ExecuteScalarAsync());

        return ExchangeRateMapper.ToDto(
            new ExchangeRate(
                lastId,
                baseCurrencyId,
                targetCurrencyId,
                rate
            ),
            baseCurrency,
            targetCurrency
        );
    }

    public async Task<bool> IsExchangeRateExistsAsync(int? baseCurrencyId, int? targetCurrencyId)
    {
        using var connection = _connectionProvider.GetConnection();

        await connection.OpenAsync();

        using var command = new SqliteCommand("SELECT * FROM ExchangeRates WHERE BaseCurrencyId = @baseCurrencyId AND TargetCurrencyId = @targetCurrencyId", connection);

        command.Parameters.AddWithValue("@baseCurrencyId", baseCurrencyId);
        command.Parameters.AddWithValue("@targetCurrencyId", targetCurrencyId);

        using var dataReader = await command.ExecuteReaderAsync();

        return await dataReader.ReadAsync();
    }

    public async Task<ExchangeRateDto?> UpdateExchangeRateDtoAsync(string pair, decimal? rate)
    {
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        var baseCurrencyCode = pair.Substring(0, 3);
        var targetCurrencyCode = pair.Substring(3, 3);
        var baseCurrencyId = await _currencyDao.GetCurrencyIdByCodeAsync(baseCurrencyCode);
        var targetCurrencyId = await _currencyDao.GetCurrencyIdByCodeAsync(targetCurrencyCode);

        using var command = new SqliteCommand("UPDATE ExchangeRates SET Rate = @rate WHERE BaseCurrencyId = @baseCurrencyId AND TargetCurrencyId = @targetCurrencyId;" +
            "SELECT ID, BaseCurrencyId, TargetCurrencyId FROM ExchangeRates WHERE BaseCurrencyId = @baseCurrencyId AND TargetCurrencyId = @targetCurrencyId;", connection);

        command.Parameters.AddWithValue("@rate", rate);
        command.Parameters.AddWithValue("@baseCurrencyId", baseCurrencyId);
        command.Parameters.AddWithValue("@targetCurrencyId", targetCurrencyId);

        using var dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            var baseCurrency = await _currencyDao.GetCurrencyByIdAsync(dataReader.GetInt32(1));
            var targetCurrency = await _currencyDao.GetCurrencyByIdAsync(dataReader.GetInt32(2));

            if (baseCurrency is null || targetCurrency is null)
            {
                return null;
            }

            return ExchangeRateMapper.ToDto(
                new ExchangeRate(
                    dataReader.GetInt32(0),
                    baseCurrency.ID,
                    targetCurrency.ID,
                    rate
                ),
                baseCurrency,
                targetCurrency
            );
        }

        return null;
    }
}
