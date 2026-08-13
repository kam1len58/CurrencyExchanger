using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.Dao;

public class DBConnectionProvider
{
    private string _connection;

    public DBConnectionProvider()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
        _connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена в файле appsettings.json");
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connection);
    }
}
