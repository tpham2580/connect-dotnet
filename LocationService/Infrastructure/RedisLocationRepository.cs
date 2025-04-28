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

    public async Task<IReadOnlyList<BusinessNearbyDto>> SearchAsync(double latitude, double longitude, uint radiusM, int? limit, CancellationToken ct)
    {
        var shape = new GeoSearchCircle(radiusM);
        var results = await _db.GeoSearchAsync(
                        key: GeoKey,
                        longitude: longitude,
                        latitude: latitude,
                        shape: shape,
                        count: limit ?? 0,
                        order: Order.Ascending,
                        options: GeoRadiusOptions.WithDistance);

        _log.LogInformation("Result has been received: \n{results}", results.ToString());

        return results.Select(r => new BusinessNearbyDto
        {
            BusinessId = long.Parse(r.Member.ToString()),
            DistanceMeters = r.Distance
        }).ToList();
    }
}
