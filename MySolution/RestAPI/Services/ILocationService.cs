namespace RestAPI.Services;
using RestAPI.Dtos;

public interface ILocationService
{
    Task<LocationResponse[]> GetNearbyAsync(LocationRequest request);
}
