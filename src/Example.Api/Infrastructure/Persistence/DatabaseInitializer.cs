using DbUp;

namespace Example.Api.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseInitializer).Assembly)
            .WithTransaction()
            .LogToConsole()
            .Build();

        if (upgrader.IsUpgradeRequired())
        {
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new InvalidOperationException(result.Error.Message);
            }
        }

        await Task.CompletedTask;
    }
}