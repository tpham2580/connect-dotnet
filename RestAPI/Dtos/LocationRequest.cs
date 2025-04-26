namespace RestAPI.Dtos;

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }

    public override string ToString()
    {
        return "{ Latitude: " + Latitude.ToString() + "; " + "Longitude: " + Longitude.ToString() + "; " + "Radius: " + Radius.ToString() + " }\n";
    }

}
