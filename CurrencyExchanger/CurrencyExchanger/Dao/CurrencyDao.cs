using CurrencyExchanger.Dao;
using CurrencyExchanger.Domain;
using CurrencyExchanger.Dto;
using CurrencyExchanger.Mappers;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger;

public class CurrencyDao
{
    private DBConnectionProvider _connectionProvider;

    public CurrencyDao(DBConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public List<Currency> GetAllCurrencies()
    {
        var currencies = new List<Currency>();
        using (var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("SELECT * FROM [Currencies]", connection);

            using (var dataReader = command.ExecuteReader())
            {
                while(dataReader.Read())
                {
                    var currency = new Currency(dataReader.GetInt32(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetString(3));
                    currencies.Add(currency);
                }
            }
        }

        return currencies;
    }

    public Currency? GetCurrencyById(int id)
    {
       using (var connection = _connectionProvider.GetConnection())
       {
            connection.Open();
            var command = new SqliteCommand("SELECT * FROM [Currencies] WHERE ID=@id", connection);
            command.Parameters.AddWithValue("@id", id);

            using (var dataReader = command.ExecuteReader())
            {
                if (dataReader.Read())
                    return new Currency(dataReader.GetInt32(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetString(3));
            }
       }

       return null;
    }

    public Currency? GetCurrencyByCode(string code)
    {
        using(var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("SELECT * FROM [Currencies] WHERE CODE=@code", connection);
            command.Parameters.AddWithValue("@code", code);

            using (var dataReader = command.ExecuteReader())
            {
                if (dataReader.Read())
                    return new Currency(dataReader.GetInt32(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetString(3));
            }
        }

        return null;
    }

    public int GetCurrencyIdByCode(string code)
    {
        using(var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("SELECT ID FROM [Currencies] WHERE CODE=@code", connection);
            command.Parameters.AddWithValue("@code", code);

            using (var dataReader = command.ExecuteReader())
            {
                if (dataReader.Read())
                    return dataReader.GetInt32(0);
            }
        }

        return 0;
    }

    public Currency AddCurrency(string name, string code, string sign)
    {
        using(var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("INSERT INTO [Currencies] (Code, FullName, Sign) VALUES (@code, @name, @sign);" +
                "SELECT last_insert_rowid();", connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@sign", sign);
            int lastId = Convert.ToInt32(command.ExecuteScalar());
            return new Currency(lastId,code,name, sign);
        }
    }

    public bool IsCurrencyExists(string code)
    {
        using(var connection = _connectionProvider.GetConnection())
        {
            connection.Open();
            var command = new SqliteCommand("SELECT * FROM [Currencies] WHERE CODE=@code", connection);
            command.Parameters.AddWithValue("@code", code);

            using (var dataReader = command.ExecuteReader())
            {
                if(dataReader.Read())
                    return true;
            }
        }

        return false;
    }
}
