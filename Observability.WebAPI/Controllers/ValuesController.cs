using Microsoft.AspNetCore.Mvc;

namespace Observability.WebAPI.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello()
    {
        int x = 10;
        int y = 0;
        int z = x / y;
        return Ok("Hello world!");
    }
}
