using CurrencyExchanger.Domain;
using CurrencyExchanger.Domain.Validators;
using CurrencyExchanger.Dto;
using CurrencyExchanger.Mappers;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.Dao;

public class ExchangeRateDao
{
    private DBConnectionProvider _connectionProvider;
    private ExchangeRateValidator _exchangeRateValidator;

    public ExchangeRateDao(DBConnectionProvider connectionProvider, ExchangeRateValidator exchangeRateValidator)
    {
        _connectionProvider = connectionProvider;
        _exchangeRateValidator = exchangeRateValidator;
    }

    public List<ExchangeRate> GetAllExchangeRates()
    {
        var exchangeRates = new List<ExchangeRate>();
        using (var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("SELECT * FROM [ExchangeRates]", connection);

            using (var dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    var exchangeRate = new ExchangeRate(dataReader.GetInt32(0), dataReader.GetInt32(1), dataReader.GetInt32(2), dataReader.GetDecimal(3));
                    exchangeRates.Add(exchangeRate);
                }
            }
        }

        return exchangeRates;
    }

    public ExchangeRateDto? GetExchangeRateByCode(string pair)
    {
        var currencyDao = new CurrencyDao(_connectionProvider);
        if(pair.Length == 6)
        {
            string baseCurrencyCode = pair.Substring(0, 3);
            string targetCurrencyCode = pair.Substring(3, 3);
            _exchangeRateValidator.ValidateCode(baseCurrencyCode, targetCurrencyCode);


            using (var connection = _connectionProvider.GetConnection())
            {
                connection.Open();
                var command = new SqliteCommand("SELECT * FROM [ExchangeRates] " +
                    "JOIN [Currencies] AS BaseCurrency ON [ExchangeRates].BaseCurrencyId = BaseCurrency.ID " +
                    "JOIN [Currencies] AS TargetCurrency ON [ExchangeRates].TargetCurrencyId = TargetCurrency.ID " +
                    "WHERE BaseCurrency.Code = @baseCurrencyCode AND TargetCurrency.Code = @targetCurrencyCode", connection);
                command.Parameters.AddWithValue("@baseCurrencyCode", baseCurrencyCode);
                command.Parameters.AddWithValue("@targetCurrencyCode", targetCurrencyCode);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        var baseCurrency = currencyDao.GetCurrencyByCode(baseCurrencyCode);
                        var targetCurrency = currencyDao.GetCurrencyByCode(targetCurrencyCode);

                        if(baseCurrency is null || targetCurrency is null)
                            return null;

                        return ExchangeRateMapper.
                            ToDto
                            (new ExchangeRate(dataReader.GetInt32(0),
                             CurrencyMapper.ToDto(baseCurrency).ID,
                             CurrencyMapper.ToDto(targetCurrency).ID,
                             dataReader.GetDecimal(3)),baseCurrency,targetCurrency);
                    }
                }
            }
        }
   
        return null;
    }

    public ExchangeRateDto? AddExchangeRate(int baseCurrencyId, int targetCurrencyId, decimal rate)
    {
        var currencyDao = new CurrencyDao(_connectionProvider);
        using (var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("INSERT INTO [ExchangeRates] (BaseCurrencyId, TargetCurrencyId, Rate) VALUES (@baseCurrencyId, @targetCurrencyId, @rate);" +
                "SELECT last_insert_rowid();", connection);
            command.Parameters.AddWithValue("@baseCurrencyId", baseCurrencyId);
            command.Parameters.AddWithValue("@targetCurrencyId", targetCurrencyId);
            command.Parameters.AddWithValue("@rate", rate);
            int lastId = Convert.ToInt32(command.ExecuteScalar());
            var baseCurrency = currencyDao.GetCurrencyById(baseCurrencyId);
            var targetCurrency = currencyDao.GetCurrencyById(targetCurrencyId);
            if (baseCurrency is null || targetCurrency is null)
                return null;

            return ExchangeRateMapper.ToDto(new ExchangeRate(lastId, baseCurrencyId, targetCurrencyId, rate), baseCurrency, targetCurrency);
        }    
    }

    public bool IsExchangeRateExists(int baseCurrencyId, int targetCurrencyId)
    {
        using(var connection  = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("SELECT * FROM [ExchangeRates] WHERE BaseCurrencyId = @baseCurrencyId AND TargetCurrencyId = @targetCurrencyId", connection);
            command.Parameters.AddWithValue("@baseCurrencyId", baseCurrencyId);
            command.Parameters.AddWithValue("@targetCurrencyId", targetCurrencyId);

            using(var dataReader =  command.ExecuteReader())
            {
                if(dataReader.Read())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public ExchangeRateDto? UpdateExchangeRateDto(string pair, decimal? rate)
    {
        if(pair.Length ==6)
        {
            using (var connection = _connectionProvider.GetConnection())
            {
                connection.Open();
                var currencyDao = new CurrencyDao(_connectionProvider);
                var baseCurrencyCode = pair.Substring(0, 3);
                var targetCurrencyCode = pair.Substring(3, 3);
                var baseCurrencyId = currencyDao.GetCurrencyIdByCode(baseCurrencyCode);
                var targetCurrencyId = currencyDao.GetCurrencyIdByCode(targetCurrencyCode);
                var command = new SqliteCommand("UPDATE [ExchangeRates] SET Rate = @rate WHERE BaseCurrencyId = @baseCurrencyId AND TargetCurrencyId = @targetCurrencyId;" +
                    "SELECT ID, BaseCurrencyId, TargetCurrencyId FROM [ExchangeRates] WHERE BaseCurrencyId = @baseCurrencyId AND TargetCurrencyId = @targetCurrencyId;", connection);
                command.Parameters.AddWithValue("@rate", rate);
                command.Parameters.AddWithValue("@baseCurrencyId", baseCurrencyId);
                command.Parameters.AddWithValue("@targetCurrencyId", targetCurrencyId);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        var baseCurrency = currencyDao.GetCurrencyById(dataReader.GetInt32(1));
                        var targetCurrency = currencyDao.GetCurrencyById(dataReader.GetInt32(2));

                        if (baseCurrency is null || targetCurrency is null)
                            return null;

                        return ExchangeRateMapper.ToDto
                        (
                            new(dataReader.GetInt32(0), baseCurrency.ID, targetCurrency.ID, rate),
                            baseCurrency,
                            targetCurrency
                        );
                    }
                }
            }
        }
      
        return null;
    }
}
