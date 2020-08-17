using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Data;
using Glaz.Server.Data.Enums;
using Glaz.Server.Data.Vuforia;
using Glaz.Server.Data.Vuforia.Responses;
using Glaz.Server.Entities;
using Glaz.Server.Models.ManageOrders;
using Glaz.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Glaz.Server.Data.Enums.Roles;

namespace Glaz.Server.Controllers
{
    [Authorize(Roles = Moderator)]
    [Route("Orders/Manage/[action]")]
    public class ManageOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GlazAccount> _userManager;
        private readonly IVuforiaService _vuforiaService;
        private readonly string _rootDirectory;
        private readonly string _bundlesDirectory = Path.Combine("Attachments","Bundles");

        public ManageOrdersController(ApplicationDbContext context,
            UserManager<GlazAccount> userManager,
            IVuforiaService vuforiaService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _vuforiaService = vuforiaService;
            _rootDirectory = webHostEnvironment.WebRootPath;
        }
        
        [Route("~/Orders/Manage")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Account)
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .OrderBy(o => o.State)
                .ToArrayAsync();

            var clientOrders = orders
                .Select(order => new ModeratorOrder(order))
                .ToArray();

            return View(clientOrders);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> EditNote(Guid id)
        {
            var comment = await _context.Orders
                .Where(o => o.Id == id)
                .Select(o => o.ModeratorComment)
                .FirstOrDefaultAsync();

            return View(new WriteNoteModel(id, comment));
        }

        [HttpPost]
        public async Task<IActionResult> SaveNote(WriteNoteModel model)
        {
            var order = await _context.Orders.FindAsync(model.Id);
            
            if (order is null)
            {
                return NotFound();
            }
            
            order.ModeratorComment = model.Comment;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ChangeState(Guid id)
        {
            var state = await _context.Orders
                .Where(o => o.Id == id)
                .Select(o => o.State)
                .FirstOrDefaultAsync();

            return View(new ChangeStateModel(id, state));
        }

        [HttpPost]
        public async Task<IActionResult> SaveState(ChangeStateModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .FirstOrDefaultAsync(o => o.Id == model.Id);

            if (order is null)
            {
                return NotFound();
            }

            if (model.State == OrderState.Published)
            {
                var targetAttachment = GetTargetAttachment(order);
                string targetId = await UploadVuforiaTarget(targetAttachment);
                await SaveTargetId(targetAttachment.Id, targetId);
            }
            else if (model.State == OrderState.Deleted)
            {
                await DeleteVuforiaTargetIfExists(GetTargetAttachment(order));
                MarkOrderEntitesAsHasToDelete(order);
            }
            
            order.State = model.State;
            _context.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// Adds new target to Vuforia database
        /// </summary>
        /// <param name="target">JSON object of new target</param>
        /// <returns>Target ID</returns>
        private async Task<string> UploadVuforiaTarget(Attachment target)
        {
            var targetModel = new TargetModel
            {
                Name = target.Label,
                Width = TargetModel.DefaultWidth,
                ImageBase64 = await ReadTargetFileAsBase64(target.Path)
            };
            return await _vuforiaService.AddTarget(targetModel);
        }
        private async Task<string> ReadTargetFileAsBase64(string filePath)
        {
            string absolutePath = Path.Combine(_rootDirectory, filePath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
            return Convert.ToBase64String(fileBytes);
        }
        private async Task SaveTargetId(Guid attachmentId, string targetId)
        {
            var dbAttachment = await _context.Attachments
                .Include(a => a.VuforiaDetails)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);
            var newDetails = new VuforiaDetails(targetId);
            _context.VuforiaDetails.Add(newDetails);
            dbAttachment.VuforiaDetails = newDetails;
            _context.Attachments.Update(dbAttachment);
            await _context.SaveChangesAsync();
        }
        private void MarkOrderEntitesAsHasToDelete(Order order)
        {
            order.State = OrderState.Deleted;
            order.HastToDelete = true;

            foreach (var orderAttachment in order.Attachments)
            {
                orderAttachment.HastToDelete = true;
                if (orderAttachment.VuforiaDetails != null)
                {
                    orderAttachment.VuforiaDetails.HasToDelete = true;
                }
            }
        }
        private async Task DeleteVuforiaTargetIfExists(Attachment target)
        {
            if (target?.VuforiaDetails != null)
            {
                await _vuforiaService.DeleteTarget(target.VuforiaDetails.TargetId);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> UploadBundles(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return View(new OrderBundles { OrderId = order.Id });
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateBundles(OrderBundles bundles)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(UploadBundles), bundles);
            }
            
            var order = await _context.Orders
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == bundles.OrderId);

            if (bundles.AndroidBundle != null)
            {
                var attachment = order.Attachments
                    .FirstOrDefault(a => a.Platform == AttachmentPlatform.Android);
                if (attachment is null)
                {
                    await CreateBundle(bundles.AndroidBundle, AttachmentPlatform.Android, order.Id);
                }
                else
                {
                    await UpdateBundle(attachment, bundles.AndroidBundle);
                }
            }
            
            if (bundles.IosBundle != null)
            {
                var attachment = order.Attachments
                    .FirstOrDefault(a => a.Platform == AttachmentPlatform.Ios);
                if (attachment is null)
                {
                    await CreateBundle(bundles.IosBundle, AttachmentPlatform.Ios, order.Id);
                }
                else
                {
                    await UpdateBundle(attachment, bundles.IosBundle);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        private async Task CreateBundle(IFormFile bundleFile, AttachmentPlatform platform, Guid orderId)
        {
            var id = Guid.NewGuid();
            var extension = Path.GetExtension(bundleFile.FileName);
            var attachment = new Attachment
            {
                Id = id,
                Account = await _userManager.GetUserAsync(User),
                Label = $"{platform:G}_{id}",
                OrderId = orderId,
                Platform = platform,
                Type = AttachmentType.Bundle,
                CreatedAt = DateTime.Now,
                Path = Path.Combine(_bundlesDirectory, $"{platform:G}_{id}{extension}")
            };
            await SaveBundleFile(bundleFile, attachment.Path);
            _context.Attachments.Add(attachment);
        }
        private async Task SaveBundleFile(IFormFile file, string path)
        {
            var absolutePath = Path.Combine(_rootDirectory, path);
            await using var stream = new FileStream(absolutePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        private async Task UpdateBundle(Attachment bundle, IFormFile bundleFile)
        {
            await SaveBundleFile(bundleFile, bundle.Path);
            _context.Attachments.Update(bundle);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> ViewOrder(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            var targetAttachemnt = GetTargetAttachment(order);
            if (targetAttachemnt.VuforiaDetails != null)
            {
                if (targetAttachemnt.VuforiaDetails.TargetVersion == -1)
                {
                    var details = await _vuforiaService.GetTargetRecord(targetAttachemnt.VuforiaDetails.TargetId);
                    await SaveTargetRating(targetAttachemnt.VuforiaDetails, details);
                }
            }

            return View(new DetailsModerOrder(order));
        }
        private async Task SaveTargetRating(VuforiaDetails details, TargetRecord record)
        {
            details.Rating = record.TrackingRating < 0 ? (byte)0 : (byte)record.TrackingRating;
            if (record.TrackingRating >= 0)
            {
                details.TargetVersion++;
            }
            _context.VuforiaDetails.Update(details);
            await _context.SaveChangesAsync();
        }

        private Attachment GetTargetAttachment(Order order)
        {
            return order.Attachments
                .First(a => a.Type == AttachmentType.Target);
        }
    }
}