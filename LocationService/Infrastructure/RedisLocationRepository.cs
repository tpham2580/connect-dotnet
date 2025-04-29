using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using LocationService.Dtos;

namespace LocationService.Infrastructure;

public sealed class RedisLocationRepository : ILocationRepository
{
    private readonly ILogger<RedisLocationRepository> _log;
    private const string GeoKey = "biz:geo";
    private readonly IDatabase _db;

    public RedisLocationRepository(IConnectionMultiplexer mux, ILogger<RedisLocationRepository> log)
    {
        _db = mux.GetDatabase();
        _log = log;
    }

    public async Task<IReadOnlyList<BusinessNearbyDto>> SearchAsync(double latitude, double longitude, uint radiusM, int limit, CancellationToken ct)
    {
        var shape = new GeoSearchCircle(radiusM);
        var results = await _db.GeoSearchAsync(
                        key: GeoKey,
                        longitude: longitude,
                        latitude: latitude,
                        shape: shape,
                        count: limit,
                        order: Order.Ascending,
                        options: GeoRadiusOptions.WithDistance);

        _log.LogInformation("Results have been received: \n{results}", results.ToString());

        _log.LogInformation("Retrieving names from ids");
        var redisKeys = results
            .Select(r => (RedisKey)$"business:{r.Member.ToString()}")
            .ToArray();
        var nameValues = await _db.StringGetAsync(redisKeys);

        _log.LogInformation("Names have been retrieved: \n{nameValues}", nameValues.ToString());
        var nearbyDtos = new List<BusinessNearbyDto>();
        for (int i = 0; i < results.Length; i++)
        {
            var name = nameValues[i].HasValue ? nameValues[i].ToString() : null;

            nearbyDtos.Add(new BusinessNearbyDto
            {
                BusinessId = long.Parse(results[i].Member.ToString()),
                Name = name,
                DistanceMeters = results[i].Distance
            });
        }

        return nearbyDtos;
    }
}
