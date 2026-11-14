using Life.Application.House;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Life.API.Controllers;

public class HouseController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<HouseStatusResponse> Get() => Ok(new HouseStatusResponse("ok"));
}
