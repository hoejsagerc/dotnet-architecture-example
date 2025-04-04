using System.Data;
using Npgsql;

namespace Example.Api.Infrastructure.Persistence;

public class NpgsqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}