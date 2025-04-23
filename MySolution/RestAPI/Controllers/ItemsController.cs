using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Services;

namespace RestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    public ItemsController(IItemService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public ActionResult<ItemResponse> GetById(Guid id)
    {
        var item = _service.GetById(id);
        return item is null ? NotFound() : Ok(item);
    }
}
