namespace LocationService.Dtos;

public class BusinessNearbyDto
{
    public long BusinessId { get; set; }
    public string? Name { get; set; }
    public double? DistanceMeters { get; set; }
}
