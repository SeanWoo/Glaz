using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Data;
using Glaz.Server.Data.Enums;
using Glaz.Server.Data.Vuforia.Responses;
using Glaz.Server.Entities;
using Glaz.Server.Models.Orders;
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
    [Authorize(Roles = Customer)]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GlazAccount> _userManager;
        private readonly IVuforiaService _vuforiaService;
        private readonly string _rootDirectory;
        private readonly string _targetsDirectory;
        private readonly string _responseFilesDirectory;

        public OrdersController(ApplicationDbContext context,
            UserManager<GlazAccount> userManager,
            IVuforiaService vuforiaService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _vuforiaService = vuforiaService;
            _rootDirectory = webHostEnvironment.WebRootPath;
            const string attachmentsDirectory = "Attachments";
            _targetsDirectory = Path.Combine(attachmentsDirectory, "Targets");
            _responseFilesDirectory = Path.Combine(attachmentsDirectory, "ResponseFiles");
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

        // GET: Orders/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .Include(o => o.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var targetAttachemnt = GetTargetAttachment(order);
            if (targetAttachemnt.VuforiaDetails.TargetVersion == -1)
            {
                var details = await _vuforiaService.GetTargetRecord(targetAttachemnt.VuforiaDetails.TargetId);
                await SaveTargetRating(targetAttachemnt.VuforiaDetails, details);
            }
            return View(new DetailsOrder(order));
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
            if (!ModelState.IsValid)
            {
                return View(orderDto);
            }

            await CreateOrderAndSaveToDatabase(orderDto);
            return RedirectToAction(nameof(Index));
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
                    ".tag.gz" => AttachmentType.Archive,
                    ".mp4" => AttachmentType.Video,
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

        // GET: Orders/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            
            return View(new EditOrder(order));
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditOrder orderDto)
        {
            if (id != orderDto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(orderDto);
            }

            try
            {
                var oldOrder = await _context.Orders
                    .Include(o => o.Attachments)
                    .Include(o => o.Account)
                    .FirstOrDefaultAsync(o => o.Account.UserName == User.Identity.Name && o.Id == id);
                await EditOrderAndSaveToDatabase(oldOrder, orderDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(orderDto.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));

        }
        private async Task EditOrderAndSaveToDatabase(Order order, EditOrder orderDto)
        {
            order.Label = orderDto.Label;
            order.Comment = orderDto.Comment;
            order.State = OrderState.Verifying;
            _context.Orders.Update(order);

            if (orderDto.TargetImage != null)
            {
                var target = await CreateAttachment(orderDto.TargetImage, true);
                await UpdateTargetAttachment(order.Id, target);
            }

            if (orderDto.ResponseFile != null)
            {
                var response = await CreateAttachment(orderDto.ResponseFile);
                await UpdateResponseAttachment(order.Id, response);
            }
            
            await _context.SaveChangesAsync();
        }
        private async Task UpdateTargetAttachment(Guid orderId, Attachment newTarget)
        {
            var target = await _context.Attachments
                .Include(a => a.VuforiaDetails)
                .FirstAsync(a => a.OrderId == orderId && a.Type == AttachmentType.Target);
            target.Label = newTarget.Label;
            target.CreatedAt = DateTime.Now;
            target.Path = newTarget.Path;

            if (target.VuforiaDetails != null)
            {
                target.VuforiaDetails.TargetVersion = -1;
            }
            
            _context.Attachments.Update(target);
        }
        private async Task UpdateResponseAttachment(Guid orderId, Attachment newResponse)
        {
            var response = await _context.Attachments
                .FirstAsync(a => a.OrderId == orderId && a.Type == AttachmentType.Archive);
            response.Label = newResponse.Label;
            response.CreatedAt = DateTime.Now;
            response.Path = newResponse.Path;
            _context.Attachments.Update(response);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(new DeleteOrder(order));
        }

        // POST: Orders/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
            await MarkOrderEntitesAsHasToDelete(order);
            
            var target = order.Attachments
                .FirstOrDefault(a => a.Type == AttachmentType.Target);
            await DeleteVuforiaTargetIfExists(target);
            
            return RedirectToAction(nameof(Index));
        }
        private async Task MarkOrderEntitesAsHasToDelete(Order order)
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
            
            _context.Update(order);
            await _context.SaveChangesAsync();
        }
        private async Task DeleteVuforiaTargetIfExists(Attachment target)
        {
            if (target?.VuforiaDetails != null)
            {
                await _vuforiaService.DeleteTarget(target.VuforiaDetails.TargetId);
            }
        }

        private bool OrderExists(Guid id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        
        private Attachment GetTargetAttachment(Order order)
        {
            return order.Attachments
                .First(a => a.Type == AttachmentType.Target);
        }
    }
}