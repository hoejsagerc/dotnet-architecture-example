using Example.Api.Infrastructure.Persistence;

public static class DatabaseServiceExtension
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddSingleton<IDbConnectionFactory>(
            provider => new NpgsqlDbConnectionFactory(connectionString!));

        services.AddSingleton(_ => new DatabaseInitializer(connectionString!));
        return services;
    }
}