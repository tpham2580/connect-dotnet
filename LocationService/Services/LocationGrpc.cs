// Services/LocationGrpc.cs
using Grpc.Core;
using StackExchange.Redis;
using Grpc.LocationService;
using LocationService.Infrastructure;

namespace LocationService.Services;

public class LocationGrpc : Grpc.LocationService.LocationService.LocationServiceBase
{
    private readonly ILocationRepository _repo;
    private readonly ILogger<LocationGrpc> _log;

    public LocationGrpc(ILocationRepository repo, ILogger<LocationGrpc> log) =>
        (_repo, _log) = (repo, log);

    public override async Task<NearbyResponse> GetNearbyBusinesses(
        NearbyRequest request, ServerCallContext context)
    {
        var results = await _repo.SearchAsync(
                      request.Latitude, request.Longitude,
                      request.RadiusM,
                      request.Limit,
                      context.CancellationToken);

        var response = new NearbyResponse
        {
            Total = results.Count
        };

        response.Businesses.AddRange(
            results.Select(r => new NearbyBusiness
            {
                BusinessId = r.BusinessId,
                Name = r.Name,
                DistanceMeters = r.DistanceMeters ?? 0
            })
        );

        return response;
    }
}
