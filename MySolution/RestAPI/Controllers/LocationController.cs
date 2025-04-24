using Microsoft.AspNetCore.Mvc;
using RestAPI.Services;
using RestAPI.Dtos;

namespace RestAPI.Controllers;

[ApiController]
[Route("location")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService locationService, ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;

    }

    [HttpPost]
    public async Task<IActionResult> PostLocation([FromBody] LocationRequest request)
    {
        _logger.LogInformation("Received location: {Lat}, {Lng}", request.Latitude, request.Longitude);

        var results = await _locationService.GetNearbyAsync(request);

        _logger.LogInformation("Returning {Count} nearby results", results.Length);

        return Ok(results);
    }
}
