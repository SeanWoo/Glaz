using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Data;
using Glaz.Server.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glaz.Server.Controllers.Api
{
    [AllowAnonymous]
    [Route("/api/[controller]")]
    public class BundlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _rootDirectory;
        
        public BundlesController(ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _rootDirectory = env.WebRootPath;
        }
        
        [HttpGet("/api/[controller]/{id}/{platform}")]
        public async Task<IActionResult> Index(string targetId, string platform)
        {
            var isParsed = Enum.TryParse(platform, true, out AttachmentPlatform enumPlatform);

            if (!isParsed)
            {
                var availablePlatforms = string.Join(", ", Enum.GetNames(typeof(AttachmentPlatform)));
                return BadRequest($"Unknown platform. Available only: {availablePlatforms}");
            }

            var orderId = await _context.Attachments
                .Include(a => a.VuforiaDetails)
                .Where(a => a.VuforiaDetails.TargetId == targetId)
                .Select(a => a.OrderId)
                .FirstOrDefaultAsync();

            if (orderId == default)
            {
                return NotFound("Order with such targetId is missing");
            }

            var attachment = await _context.Attachments
                .Include(a => a.VuforiaDetails)
                .FirstOrDefaultAsync(a => a.OrderId == orderId && a.Platform == enumPlatform);

            if (attachment is null)
            {
                return NotFound("Bundle for this platform is missing");
            }
                
            var path = Path.Combine(_rootDirectory, attachment.Path);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(fileBytes, "application/octet-stream");
        }
    }
}