namespace BusinessService.Models;

public interface IBusinessModel
{
    Int64? Id { get; set; }
    string Name { get; set; }
    string Address { get; set; }
    string City { get; set; }
    string State { get; set; }
    string Country { get; set; }
    double Latitude { get; set; }
    double Longitude { get; set; }
}

