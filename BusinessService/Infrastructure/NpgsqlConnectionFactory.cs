using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BusinessService.Infrastructure;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _connectionString = config.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Postgres connection string is missing in configuration.");
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
