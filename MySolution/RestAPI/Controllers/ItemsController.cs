using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RestAPI.Dtos;
using RestAPI.Services;

namespace RestAPI.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    private ItemsController(IItemService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    private async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }
}
