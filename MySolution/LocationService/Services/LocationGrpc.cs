// Services/LocationGrpc.cs
using Grpc.Core;
using StackExchange.Redis;
using LocationService;
using LocationService.Infrastructure;

namespace LocationService.Services;

public class LocationGrpc : LocationService.LocationServiceBase
{
    private readonly ILocationRepository _repo;
    private readonly ILogger<LocationGrpc> _log;

    public LocationGrpc(ILocationRepository repo, ILogger<LocationGrpc> log) =>
        (_repo, _log) = (repo, log);

    public override async Task<NearbyResponse> GetNearbyBusinesses(
        NearbyRequest request, ServerCallContext context)
    {
        var ids = await _repo.SearchAsync(
                      request.Latitude, request.Longitude,
                      (int)request.RadiusM,
                      request.Limit == 0 ? null : (int?)request.Limit,
                      context.CancellationToken);

        return new NearbyResponse
        {
            Total = (uint)ids.Count,
            BusinessIds = { ids }
        };
    }
}
