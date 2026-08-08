using System.Data;
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

    /// <summary>
    /// latitude/longitude are nullable in the schema, so a row with missing coordinates
    /// must not fail the read. Every query below selects the same column order, so this
    /// is the single place that maps a business row.
    /// </summary>
    private static BusinessModel MapBusiness(NpgsqlDataReader reader) => new BusinessModel
    {
        Id = reader.GetInt64(0),
        Name = reader.GetString(1),
        Address = reader.GetString(2),
        City = reader.GetString(3),
        State = reader.GetString(4),
        Country = reader.GetString(5),
        Latitude = reader.IsDBNull(6) ? 0d : reader.GetDouble(6),
        Longitude = reader.IsDBNull(7) ? 0d : reader.GetDouble(7)
    };

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

        return MapBusiness(reader);
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
            results.Add(MapBusiness(reader));
        }

        return results;
    }

    /// <summary>
    /// Keyset ("seek") pagination: rows are selected by cursor rather than OFFSET so the
    /// cost of a page does not grow with how deep into the table it sits.
    /// </summary>
    public async Task<(List<BusinessModel> Businesses, long Total, bool HasMore)> GetBusinessesAsync(
        int limit,
        long after,
        CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // The page and the count must observe the same snapshot, otherwise Total can
        // contradict the rows returned when businesses are written concurrently.
        await using var transaction = await connection.BeginTransactionAsync(
            IsolationLevel.RepeatableRead,
            cancellationToken);

        const string pageQuery = @"
            SELECT business_id, name, address, city, state, country, latitude, longitude
            FROM business
            WHERE business_id > @after
            ORDER BY business_id
            LIMIT @limit;
        ";

        var results = new List<BusinessModel>();

        await using (var cmd = new NpgsqlCommand(pageQuery, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@after", after);
            // Read one row beyond the page to detect a further page without a second query.
            cmd.Parameters.AddWithValue("@limit", (long)limit + 1);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(MapBusiness(reader));
            }
        }

        var hasMore = results.Count > limit;
        if (hasMore)
        {
            results.RemoveAt(results.Count - 1);
        }

        const string countQuery = "SELECT COUNT(*) FROM business;";

        long total;
        await using (var countCmd = new NpgsqlCommand(countQuery, connection, transaction))
        {
            total = (long)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
        }

        await transaction.CommitAsync(cancellationToken);

        return (results, total, hasMore);
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

        var created = MapBusiness(reader);
        _log.LogInformation("DB call finished. Inserted row: \n{@business}", created);
        return created;
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

        var updated = MapBusiness(reader);
        _log.LogInformation("DB call finished. Updated row: \n{@business}", updated);
        return updated;
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
