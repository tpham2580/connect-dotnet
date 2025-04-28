using RestAPI.Dtos;

namespace RestAPI.Services;

public interface ILocationSearchService
{
    Task<LocationListResponse> GetNearbyAsync(LocationRequest request);
}
