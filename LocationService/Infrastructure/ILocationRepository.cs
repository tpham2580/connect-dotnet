using LocationService.Dtos;

namespace LocationService.Infrastructure;

public interface ILocationRepository
{
    Task<IReadOnlyList<BusinessNearbyDto>> SearchAsync(
        double latitude, double longitude, uint radiusM, int? limit, CancellationToken ct);
}

