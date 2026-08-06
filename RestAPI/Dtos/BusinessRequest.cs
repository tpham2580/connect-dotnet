using System.ComponentModel.DataAnnotations;

namespace RestAPI.Dtos;

public sealed class BusinessRequest
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public required string Address { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string City { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string State { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Country { get; init; }

    [Range(-90.0, 90.0)]
    public required double Latitude { get; init; }

    [Range(-180.0, 180.0)]
    public required double Longitude { get; init; }
}
