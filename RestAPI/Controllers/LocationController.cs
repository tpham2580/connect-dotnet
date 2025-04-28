using Microsoft.AspNetCore.Mvc;
using RestAPI.Services;
using RestAPI.Dtos;

namespace RestAPI.Controllers;

[ApiController]
[Route("v1/search")]
public class LocationController : ControllerBase
{
    private const uint DEFAULT_RADIUS = 5000;
    private const int DEFAULT_LIMIT = 100;

    private readonly ILocationSearchService _locationSearchService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationSearchService locationSearchService, ILogger<LocationController> logger)
    {
        _locationSearchService = locationSearchService;
        _logger = logger;

    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] uint radius = DEFAULT_RADIUS,
            [FromQuery] int limit = DEFAULT_LIMIT)
    {
        _logger.LogInformation("Received location: {Lat}, {Lng}", latitude, longitude);

        var request = new LocationRequest
        {
            Latitude = latitude,
            Longitude = longitude,
            Radius = radius,
            Limit = limit
        };

        var response = await _locationSearchService.GetNearbyAsync(request);

        _logger.LogInformation("Returning nearby results: {Response}", response);

        return Ok(response);
    }
}
