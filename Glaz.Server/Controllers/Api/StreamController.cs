using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Controllers.Api
{
    [Route("/api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly string _rootDirectoryPath;
        
        public StreamController(IWebHostEnvironment webHostEnvironment)
        {
            _rootDirectoryPath = webHostEnvironment.WebRootPath;
        }
        
        [HttpGet("{filename}")]
        public IActionResult GetVideoStream(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return BadRequest();
            }

            string videoFilePath = Path.Combine(_rootDirectoryPath, $"Videos/{filename}.mp4");
            if (!System.IO.File.Exists(videoFilePath))
            {
                return BadRequest();
            }
            
            return PhysicalFile(videoFilePath, "application/octet-stream", true);
        }
    }
}