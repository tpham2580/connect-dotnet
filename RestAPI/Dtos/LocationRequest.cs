namespace RestAPI.Dtos;

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public uint Radius { get; set; }
    public int Limit { get; set; }

    public override string ToString()
    {
        return "{ Latitude: " + Latitude.ToString() + "; " + "Longitude: " + Longitude.ToString() + "; " + "Radius: " + Radius.ToString() + "; " + "Limit: " + Limit.ToString() + " }\n";
    }

}
