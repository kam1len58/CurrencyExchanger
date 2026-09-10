using CurrencyExchanger.Models;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.DAL.Dao;

public class CurrencyDao(DBConnectionProvider connectionProvider)
{
    private readonly DBConnectionProvider _connectionProvider = connectionProvider;

    public async Task<List<Currency>> GetAllCurrenciesAsync()
    {
        var currencies = new List<Currency>();
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        var query = "SELECT * FROM Currencies";
        using var command = new SqliteCommand(query, connection);
        using var dataReader = await command.ExecuteReaderAsync();

        while (await dataReader.ReadAsync())
        {
            var currency = new Currency(
                dataReader.GetInt32(0),
                dataReader.GetString(1),
                dataReader.GetString(2),
                dataReader.GetString(3)
            );

            currencies.Add(currency);
        }

        return currencies;
    }

    public async Task<Currency?> GetCurrencyByIdAsync(int? id)
    {
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        var query = "SELECT * FROM Currencies WHERE ID=@id";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        using var dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            return new Currency(
                dataReader.GetInt32(0),
                dataReader.GetString(1),
                dataReader.GetString(2),
                dataReader.GetString(3)
            );
        }

        return null;
    }

    public async Task<Currency?> GetCurrencyByCodeAsync(string code)
    {
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        var query = "SELECT * FROM Currencies WHERE CODE=@code";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@code", code);
        using var dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            return new Currency(
                dataReader.GetInt32(0),
                dataReader.GetString(1),
                dataReader.GetString(2),
                dataReader.GetString(3)
            );
        }

        return null;
    }

    public async Task<int?> GetCurrencyIdByCodeAsync(string code)
    {
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        var query = "SELECT ID FROM Currencies WHERE CODE=@code";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@code", code);
        using var dataReader = await command.ExecuteReaderAsync();
        int? currency = await dataReader.ReadAsync() ? dataReader.GetInt32(0) : null;

        return currency;
    }

    public async Task<Currency> AddCurrencyAsync(string name, string code, string sign)
    {
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        var query = """
            INSERT INTO Currencies (Code, FullName, SIGN)
            VALUES (@code, @name, @sign);

            SELECT LAST_INSERT_ROWID();
            """;
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@sign", sign);
        int lastId = Convert.ToInt32(await command.ExecuteScalarAsync());

        return new Currency(lastId, code, name, sign);
    }

    public async Task<bool> IsCurrencyExistsAsync(string code)
    {
        using var connection = _connectionProvider.GetConnection();
        await connection.OpenAsync();

        using var command = new SqliteCommand("SELECT * FROM Currencies WHERE CODE=@code", connection);
        command.Parameters.AddWithValue("@code", code);
        using var dataReader = await command.ExecuteReaderAsync();

        return await dataReader.ReadAsync();
    }
}
