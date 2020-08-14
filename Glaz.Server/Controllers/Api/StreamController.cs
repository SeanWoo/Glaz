using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Controllers.Api
{
    [Route("/api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public StreamController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        
        [HttpGet("{filename}")]
        public FileResult GetVideoStream(string filename)
        {
            return PhysicalFile(Path.Combine(_webHostEnvironment.WebRootPath, $"Videos/{filename}.mp4"), "application/octet-stream", true);
        }
    }
}