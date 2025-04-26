namespace LocationService.Infrastructure;

public interface ILocationRepository
{
    Task<IReadOnlyList<long>> SearchAsync(
        double latitude, double longitude, int radiusM, int? limit, CancellationToken ct);
}

