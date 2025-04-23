using Microsoft.AspNetCore.Mvc;
using RestAPI.Models;

namespace RestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private static readonly List<Item> Items = new();
}
