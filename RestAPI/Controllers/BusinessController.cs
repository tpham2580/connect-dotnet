using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Services;

namespace RestAPI.Controllers;

[ApiController]
[Route("v1/businesses")]
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
        [FromQuery, Range(1, int.MaxValue)] int page = DefaultPage,
        [FromQuery, Range(1, MaxPageSize)] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if ((long)(page - 1) * pageSize > int.MaxValue)
        {
            ModelState.AddModelError(
                nameof(page),
                "The requested page exceeds the supported paging range.");
            return ValidationProblem(ModelState);
        }

        var response = await _businessService.ListAsync(page, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var business = await _businessService.GetByIdAsync(id, cancellationToken);
        if (business == null)
        {
            return NotFound();
        }

        return Ok(business);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] BusinessRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _businessService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] BusinessRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _businessService.UpdateAsync(id, request, cancellationToken);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _businessService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
