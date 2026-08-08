namespace BusinessService.Models;

public class BusinessModel
{
    public Int64? Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string Country { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}
