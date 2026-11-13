using Application.House;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class HouseController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<HouseStatusResponse> Get() => Ok(new HouseStatusResponse("ok"));
}
