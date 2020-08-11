using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Data;
using Glaz.Server.Data.Enums;
using Glaz.Server.Entities;
using Glaz.Server.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Glaz.Server.Data.Enums.Roles;

namespace Glaz.Server.Controllers
{
    [Authorize(Roles = Customer)]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _responseFilesDirectory;

        private readonly string _rootDirectory;
        private readonly string _targetsDirectory;
        private readonly UserManager<GlazAccount> _userManager;

        public OrdersController(ApplicationDbContext context,
            UserManager<GlazAccount> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;

            _rootDirectory = webHostEnvironment.WebRootPath;
            const string attachmentsDirectory = "Attachments";
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
                .Include(o => o.Account)
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .Where(o => o.State != OrderState.Deleted && o.Account.UserName == User.Identity.Name)
                .ToArrayAsync();

            var clientOrders = orders.Select(order => new ClientOrder(order)).ToArray();

            return View(clientOrders);
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
            var response = await CreateAttachment(orderDto.ResponseFile);
            newOrder.Attachments = new List<Attachment> {target, response};
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
            var extension = Path.GetExtension(file.FileName);
            var path = Path.Combine(_targetsDirectory, $"{id}{extension}");
            var absolutePath = Path.Combine(_rootDirectory, path);
            await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return path;
        }

        private async Task<string> SaveResponseFile(IFormFile file, Guid id)
        {
            var extension = Path.GetExtension(file.FileName);
            var path = Path.Combine(_responseFilesDirectory, $"{id}{extension}");
            var absolutePath = Path.Combine(_rootDirectory, path);
            await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return path;
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
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Label,Comment,ModeratorComment")]
            Order order)
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

                    throw;
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
        [HttpPost]
        [ActionName("Delete")]
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