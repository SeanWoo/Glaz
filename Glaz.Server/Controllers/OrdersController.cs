using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Glaz.Server.Data;
using Glaz.Server.Data.Enums;
using Glaz.Server.Data.Vuforia;
using Glaz.Server.Data.Vuforia.Responses;
using Glaz.Server.Entities;
using Glaz.Server.Models.Orders;
using Glaz.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Glaz.Server.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GlazAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IVuforiaService _vuforiaService;

        private readonly string _rootDirectory;
        private readonly string _targetsDirectory;
        private readonly string _responseFilesDirectory;

        public OrdersController(ApplicationDbContext context,
            UserManager<GlazAccount> userManager,
            IWebHostEnvironment webHostEnvironment,
            IVuforiaService vuforiaService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _vuforiaService = vuforiaService;

            _rootDirectory = _webHostEnvironment.WebRootPath;
            var attachmentsDirectory = "Attachments";
            _targetsDirectory = Path.Combine(attachmentsDirectory, "Targets");
            _responseFilesDirectory = Path.Combine(attachmentsDirectory, "ResponseFiles");
            CreateMissingDirectories(
                Path.Combine(_rootDirectory, _targetsDirectory),
                Path.Combine(_rootDirectory, _responseFilesDirectory));
        }
        private void CreateMissingDirectories(params string[] paths)
        {
            foreach (var path in paths)
            {
                Directory.CreateDirectory(path);
            }
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(order => order.Attachments)
                .Include(order => order.Details)
                .Where(o => o.State != OrderState.Deleted)
                .ToArrayAsync();

            var clientOrders = orders.Select(order => new ClientOrder(order)).ToArray();

            return View(clientOrders);
        }
        private async Task SaveTargetRatings(Guid detailsId, TargetRecord record)
        {
            var details = await _context.VuforiaDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == detailsId);
            details.Rating = record.TrackingRating < 0 ? (byte)0 : (byte)record.TrackingRating;
            if (record.TrackingRating >= 0)
            {
                details.TargetVersion++;
            }
            _context.VuforiaDetails.Update(details);
            await _context.SaveChangesAsync();
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrder orderDto)
        {
            if (ModelState.IsValid)
            {
                await CreateOrderAndSaveToDatabase(orderDto);
                return RedirectToAction(nameof(Index));
            }
            return View(orderDto);
        }
        private async Task CreateOrderAndSaveToDatabase(CreateOrder orderDto)
        {
            var newOrder = new Order
            {
                Id = Guid.NewGuid(),
                Label = orderDto.Label,
                Comment = orderDto.Comment,
                Account = await _userManager.GetUserAsync(User),
                State = OrderState.Verifying
            };
            var target = await CreateAttachment(orderDto.TargetImage, true);
            var response = await CreateAttachment(orderDto.ResponseFile, false);
            newOrder.Attachments = new List<Attachment> { target, response };
            _context.Add(newOrder);
            await _context.SaveChangesAsync();
        }
        private async Task<Attachment> CreateAttachment(IFormFile file, bool isTarget = false)
        {
            var newAttachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Account = await _userManager.GetUserAsync(User),
                CreatedAt = DateTime.Now
            };

            if (isTarget)
            {
                newAttachment.Path = await SaveTargetFile(file, newAttachment.Id);
                newAttachment.Type = AttachmentType.Target;
                newAttachment.Label = $"target_{newAttachment.Id}";
            }
            else
            {
                newAttachment.Path = await SaveResponseFile(file, newAttachment.Id);
                var extension = Path.GetExtension(file.FileName);
                newAttachment.Type = extension switch
                {
                    ".zip" => AttachmentType.Archive,
                    ".rar" => AttachmentType.Archive,
                    ".7z" => AttachmentType.Archive,
                    ".mp4" => AttachmentType.Video,
                    ".json" => AttachmentType.Model,
                    ".txt" => AttachmentType.UI,
                    _ => AttachmentType.None
                };
                newAttachment.Label = $"client_{newAttachment.Id}";
            }

            return newAttachment;
        }
        private async Task<string> SaveTargetFile(IFormFile file, Guid id)
        {
            string extension = Path.GetExtension(file.FileName);
            var path = Path.Combine(_targetsDirectory, $"{id}{extension}");
            var absolutePath = Path.Combine(_rootDirectory, path);
            await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return path;
        }
        private async Task<string> SaveResponseFile(IFormFile file, Guid id)
        {
            string extension = Path.GetExtension(file.FileName);
            var path = Path.Combine(_responseFilesDirectory, $"{id}{extension}");
            var absolutePath = Path.Combine(_rootDirectory, path);
            await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return path;
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
        private async Task<string> ReadTargetFileAsBase64(string path)
        {
            string absolutePath = Path.Combine(_rootDirectory, path);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
            return Convert.ToBase64String(fileBytes);
        }
        private async Task SaveTargetId(Guid orderId, string targetId)
        {
            var dbOrder = await _context.Orders.FindAsync(orderId);
            var newDetails = new VuforiaDetails(targetId);
            _context.VuforiaDetails.Add(newDetails);
            dbOrder.Details = newDetails;
            _context.Orders.Update(dbOrder);
            await _context.SaveChangesAsync();
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Label,Comment,ModeratorComment")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(Guid id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
