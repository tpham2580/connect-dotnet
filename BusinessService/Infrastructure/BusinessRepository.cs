using Npgsql;
using BusinessService.Models;

namespace BusinessService.Infrastructure;

public class BusinessRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<BusinessRepository> _log;

    public BusinessRepository(IDbConnectionFactory connectionFactory, ILogger<BusinessRepository> log)
    {
        _connectionFactory = connectionFactory;
        _log = log;
    }

    public async Task<BusinessModel?> GetBusinessByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string query = @"
            SELECT business_id, name, address, city, state, country, latitude, longitude
            FROM business
            WHERE business_id = @id;
        ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BusinessModel
        {
            Id = reader.GetInt64(0),
            Name = reader.GetString(1),
            Address = reader.GetString(2),
            City = reader.GetString(3),
            State = reader.GetString(4),
            Country = reader.GetString(5),
            Latitude = reader.GetDouble(6),
            Longitude = reader.GetDouble(7),
        };
    }

    public async Task<List<BusinessModel>> GetAllBusinessesByIdsAsync(
        List<long> ids,
        CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string query = @"
            SELECT business_id, name, address, city, state, country, latitude, longitude
            FROM business
            WHERE business_id = ANY(@ids);
            ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@ids", ids);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var results = new List<BusinessModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BusinessModel
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Address = reader.GetString(2),
                City = reader.GetString(3),
                State = reader.GetString(4),
                Country = reader.GetString(5),
                Latitude = reader.GetDouble(6),
                Longitude = reader.GetDouble(7)
            });
        }

        return results;
    }

    public async Task<(List<BusinessModel> Businesses, long Total)> GetBusinessesAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string pageQuery = @"
            SELECT business_id, name, address, city, state, country, latitude, longitude
            FROM business
            ORDER BY business_id
            LIMIT @limit OFFSET @offset;
        ";

        var results = new List<BusinessModel>();

        await using (var cmd = new NpgsqlCommand(pageQuery, connection))
        {
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new BusinessModel
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Address = reader.GetString(2),
                    City = reader.GetString(3),
                    State = reader.GetString(4),
                    Country = reader.GetString(5),
                    // latitude/longitude are nullable in the DB; coalesce NULL to 0 so a single
                    // row with missing coordinates cannot fail the whole page read (see TASK-8).
                    Latitude = reader.IsDBNull(6) ? 0d : reader.GetDouble(6),
                    Longitude = reader.IsDBNull(7) ? 0d : reader.GetDouble(7)
                });
            }
        }

        const string countQuery = "SELECT COUNT(*) FROM business;";

        await using var countCmd = new NpgsqlCommand(countQuery, connection);
        var total = (long)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);

        return (results, total);
    }

    public async Task<BusinessModel?> CreateBusinessAsync(
        BusinessModel business,
        CancellationToken cancellationToken)
    {
        _log.LogInformation("Received Business Model: \n{@business}", business);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string query = @"
            INSERT INTO business (
                name, address, city, state, country, latitude, longitude
            ) VALUES (
                @name, @address, @city, @state, @country, @latitude, @longitude
            )
            RETURNING business_id, name, address, city, state, country, latitude, longitude;
        ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@name", business.Name);
        cmd.Parameters.AddWithValue("@address", business.Address);
        cmd.Parameters.AddWithValue("@city", business.City);
        cmd.Parameters.AddWithValue("@state", business.State);
        cmd.Parameters.AddWithValue("@country", business.Country);
        cmd.Parameters.AddWithValue("@latitude", business.Latitude);
        cmd.Parameters.AddWithValue("@longitude", business.Longitude);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        _log.LogInformation("DB call finished. Inserted row: " +
            "{Id}, {Name}, {Address}, {City}, {State}, {Country}, {Latitude}, {Longitude}",
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetDouble(6),
            reader.GetDouble(7));

        return new BusinessModel
        {
            Id = reader.GetInt64(0),
            Name = reader.GetString(1),
            Address = reader.GetString(2),
            City = reader.GetString(3),
            State = reader.GetString(4),
            Country = reader.GetString(5),
            Latitude = reader.GetDouble(6),
            Longitude = reader.GetDouble(7),
        };
    }

    public async Task<BusinessModel?> UpdateBusinessAsync(
        BusinessModel business,
        CancellationToken cancellationToken)
    {
        _log.LogInformation("Received Business Model: \n{@business}", business);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string query = @"
            UPDATE business
            SET
                name = @name,
                address = @address,
                city = @city,
                state = @state,
                country = @country,
                latitude = @latitude,
                longitude = @longitude
            WHERE business_id = @id
            RETURNING business_id, name, address, city, state, country, latitude, longitude;
        ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id", business.Id);
        cmd.Parameters.AddWithValue("@name", business.Name);
        cmd.Parameters.AddWithValue("@address", business.Address);
        cmd.Parameters.AddWithValue("@city", business.City);
        cmd.Parameters.AddWithValue("@state", business.State);
        cmd.Parameters.AddWithValue("@country", business.Country);
        cmd.Parameters.AddWithValue("@latitude", business.Latitude);
        cmd.Parameters.AddWithValue("@longitude", business.Longitude);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        _log.LogInformation("DB call finished. Inserted row: " +
            "{Id}, {Name}, {Address}, {City}, {State}, {Country}, {Latitude}, {Longitude}",
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetDouble(6),
            reader.GetDouble(7));

        return new BusinessModel
        {
            Id = reader.GetInt64(0),
            Name = reader.GetString(1),
            Address = reader.GetString(2),
            City = reader.GetString(3),
            State = reader.GetString(4),
            Country = reader.GetString(5),
            Latitude = reader.GetDouble(6),
            Longitude = reader.GetDouble(7),
        };
    }

    public async Task<bool> DeleteBusinessByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string query = @"
            DELETE FROM business
            WHERE business_id = @id;
        ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id", id);

        var affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        _log.LogInformation("DB finished being called. Number of affected rows: {@affectedRows}", affectedRows);
        return affectedRows > 0;
    }

}
