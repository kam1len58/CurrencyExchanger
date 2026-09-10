using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.DAL.Dao;

public class DBConnectionProvider(string connectionString)
{
    private readonly string _connection = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public SqliteConnection GetConnection() => new SqliteConnection(_connection);
}
