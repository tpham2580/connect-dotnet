namespace RestAPI.Dtos;

public class LocationResponse
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double DistanceMeters { get; set; }

    public override string ToString()
    {
        return "{ id: " + BusinessId.ToString() + "; " + "name: " + Name + "; " + "distance: " + DistanceMeters.ToString() + " }\n";
    }
}
