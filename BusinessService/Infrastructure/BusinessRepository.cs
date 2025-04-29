using Npgsql;

namespace BusinessService.Infrastructure;

public class BusinessRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BusinessRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<BusinessEntity?> GetBusinessByIdAsync(long id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT business_id, name, address, city, state, country, latitude, longitude
            FROM business
            WHERE business_id = @id;
        ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new BusinessEntity
        {
            BusinessId = reader.GetInt64(0),
            Name = reader.GetString(1),
            Address = reader.GetString(2),
            City = reader.GetString(3),
            State = reader.GetString(4),
            Country = reader.GetString(5),
            Latitude = reader.GetDouble(6),
            Longitude = reader.GetDouble(7),
        };
    }
}
