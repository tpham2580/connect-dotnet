using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Services;

namespace RestAPI.Controllers;

[ApiController]
[Route("v1/businesses")]
[Produces("application/json")]
public class BusinessController : ControllerBase
{
    private const long DefaultAfter = 0;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IBusinessService _businessService;
    private readonly ILogger<BusinessController> _logger;

    public BusinessController(IBusinessService businessService, ILogger<BusinessController> logger)
    {
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Lists businesses using keyset pagination. Pass the previous response's
    /// <c>nextCursor</c> as <c>after</c> to fetch the following page.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BusinessListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBusinesses(
        [FromQuery, Range(0, long.MaxValue)] long after = DefaultAfter,
        [FromQuery, Range(1, MaxPageSize)] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _businessService.ListAsync(after, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:long:min(1)}")]
    [ProducesResponseType(typeof(BusinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(BusinessResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] BusinessRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _businessService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long:min(1)}")]
    [ProducesResponseType(typeof(BusinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpDelete("{id:long:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
