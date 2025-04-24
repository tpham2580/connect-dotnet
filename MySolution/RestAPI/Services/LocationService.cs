using RestAPI.Dtos;
namespace RestAPI.Services;

public class LocationService : ILocationService
{
    public async Task<LocationResponse[]> GetNearbyAsync(LocationRequest request)
    {
        // TODO: Set up gRPC Client. Using Mock data for now.
        await Task.Delay(50);
        return new[]
        {
            new LocationResponse { Name = "Mock Place A", DistanceMeters = 124.5 },
            new LocationResponse { Name = "Mock Place B", DistanceMeters = 452.3 }
        };
    }
}
