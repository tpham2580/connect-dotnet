using RestAPI.Dtos;
using Grpc.LocationService;
namespace RestAPI.Services;

public class LocationSearchService : ILocationSearchService
{
    private readonly ILogger<LocationSearchService> _logger;
    private readonly Grpc.LocationService.LocationService.LocationServiceClient _client;

    public LocationSearchService(ILogger<LocationSearchService> logger, Grpc.LocationService.LocationService.LocationServiceClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<LocationListResponse> GetNearbyAsync(LocationRequest request)
    {
        _logger.LogInformation("Request has been received: \n{request}", request.ToString());

        var grpcRequest = new NearbyRequest
        {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusM = request.Radius,
            Limit = request.Limit
        };

        var grpcResponse = await _client.GetNearbyBusinessesAsync(grpcRequest);

        _logger.LogInformation("Result has been received: \n{results}", grpcResponse.ToString());

        return new LocationListResponse
        {
            Total = grpcResponse.Total,
            Businesses = grpcResponse.Businesses
                .Select(o => new LocationResponse
                {
                    BusinessId = o.BusinessId,
                    Name = o.Name,
                    DistanceMeters = o.DistanceMeters
                })
                .ToList()
        };
    }
}
