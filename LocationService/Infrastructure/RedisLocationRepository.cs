using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace LocationService.Infrastructure;

public sealed class RedisLocationRepository : ILocationRepository
{
    private const string GeoKey = "biz:geo";
    private readonly IDatabase _db;

    public RedisLocationRepository(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

    public async Task<IReadOnlyList<long>> SearchAsync(double latitude, double longitude, int radiusM, int? limit, CancellationToken ct)
    {
        var shape = new GeoSearchCircle(radiusM);
        var results = await _db.GeoSearchAsync(
                        key: GeoKey,
                        longitude: longitude,
                        latitude: latitude,
                        shape: shape,
                        count: limit ?? -1,
                        order: Order.Ascending);

        return results.Select(h => (long)h.Member!).ToList();
    }
}
