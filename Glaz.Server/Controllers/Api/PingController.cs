using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Controllers.Api
{
    [Route("api/Ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        public IActionResult Ping() => Ok();
    }
}