using Life.API.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Life.API.Tests.Security;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class JwtProbeController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
