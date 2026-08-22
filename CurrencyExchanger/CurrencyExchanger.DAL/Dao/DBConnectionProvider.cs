using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace CurrencyExchanger.DAL.Dao;

public class DBConnectionProvider
{
    private readonly string _connection;

    public DBConnectionProvider()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
        _connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена в файле appsettings.json");
    }

    public SqliteConnection GetConnection() => new SqliteConnection(_connection);
}
