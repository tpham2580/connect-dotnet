namespace RestAPI.Services;
using RestAPI.Dtos;

public interface ILocationService
{
    Task<LocationListResponse> GetNearbyAsync(LocationRequest request);
}
