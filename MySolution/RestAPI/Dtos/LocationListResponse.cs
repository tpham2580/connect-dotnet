namespace RestAPI.Dtos;

public class LocationListResponse
{
    public int Total { get; set; }
    public List<LocationResponse> Businesses { get; set; } = new();
}
