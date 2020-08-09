using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private IWebHostEnvironment _webHostEnvironment;
        public StreamController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("")]
        public FileResult GetVideoStream(string nameFile)
        {
            return PhysicalFile(Path.Combine(_webHostEnvironment.ContentRootPath, $"Videos/{nameFile}.MP4"), "application/octet-stream", true);
        }
    }
}