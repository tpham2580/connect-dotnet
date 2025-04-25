namespace RestAPI.Dtos;

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }
}
