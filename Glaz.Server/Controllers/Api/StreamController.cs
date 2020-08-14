using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public StreamController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("")]
        public FileResult GetVideoStream(string nameFile)
        {
            return PhysicalFile(Path.Combine(_webHostEnvironment.WebRootPath, $"Videos/{nameFile}.mp4"), "application/octet-stream", true);
        }
    }
}