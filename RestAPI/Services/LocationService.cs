using RestAPI.Dtos;
namespace RestAPI.Services;

public class LocationService : ILocationService
{
    private readonly ILogger<LocationService> _logger;

    public LocationService(ILogger<LocationService> logger)
    {
        _logger = logger;
    }

    public async Task<LocationListResponse> GetNearbyAsync(LocationRequest request)
    {
        _logger.LogInformation("Request has been received: \n{request}", request.ToString());

        // TODO: Set up gRPC Client. Using Mock data for now.
        await Task.Delay(50);
        var results = new List<LocationResponse>
        {
            new() { BusinessId = 123, Name = "Mock Place A", DistanceMeters = 124.5 },
            new() { BusinessId = 420, Name = "Mock Place B", DistanceMeters = 452.3 },
            new() { BusinessId = 140, Name = "Mock Place C", DistanceMeters = 700.0 },
        };

        string resultsStr = "[ " + String.Join(", ", results.Select(o => o.ToString())) + " ]";
        _logger.LogInformation("Result has been received: \n{results}", resultsStr);

        return new LocationListResponse
        {
            Total = results.Count,
            Businesses = results
        };
    }
}
