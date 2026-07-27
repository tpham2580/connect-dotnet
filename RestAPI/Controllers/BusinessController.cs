using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Services;

namespace RestAPI.Controllers;

[ApiController]
[Route("businesses")]
public class BusinessController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IBusinessService _businessService;
    private readonly ILogger<BusinessController> _logger;

    public BusinessController(IBusinessService businessService, ILogger<BusinessController> logger)
    {
        _businessService = businessService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetBusinesses(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        page = page < 1 ? DefaultPage : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var response = await _businessService.ListAsync(page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var business = await _businessService.GetByIdAsync(id);
        if (business == null)
        {
            return NotFound();
        }

        return Ok(business);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BusinessRequest request)
    {
        var created = await _businessService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] BusinessRequest request)
    {
        var updated = await _businessService.UpdateAsync(id, request);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _businessService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
