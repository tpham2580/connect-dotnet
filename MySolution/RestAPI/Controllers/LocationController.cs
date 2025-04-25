using Microsoft.AspNetCore.Mvc;
using RestAPI.Services;
using RestAPI.Dtos;

namespace RestAPI.Controllers;

[ApiController]
[Route("v1/search")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService locationService, ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;

    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] int radius = 5000)
    {
        _logger.LogInformation("Received location: {Lat}, {Lng}", latitude, longitude);

        var request = new LocationRequest
        {
            Latitude = latitude,
            Longitude = longitude,
            Radius = radius
        };

        var response = await _locationService.GetNearbyAsync(request);

        _logger.LogInformation("Returning nearby results: {Response}", response);

        return Ok(response);
    }
}
