using Application.House;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class HouseController : ApiControllerBase
{
    [HttpGet]
    public ActionResult<HouseStatusResponse> Get() => Ok(new HouseStatusResponse("ok"));
}
