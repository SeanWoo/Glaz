using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Controllers.Api
{
    [Route("api/Ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        public IActionResult Index() => Ok();
    }
}