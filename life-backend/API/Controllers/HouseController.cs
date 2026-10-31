using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class HouseController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
