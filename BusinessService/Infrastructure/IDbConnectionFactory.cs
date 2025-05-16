using Npgsql;

namespace BusinessService.Infrastructure;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}
