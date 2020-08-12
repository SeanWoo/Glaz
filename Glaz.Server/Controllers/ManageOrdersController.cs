using System;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Data;
using Glaz.Server.Entities;
using Glaz.Server.Models.ManageOrders;
using Glaz.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glaz.Server.Controllers
{
    [Authorize]
    public class ManageOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GlazAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IVuforiaService _vuforiaService;

        public ManageOrdersController(ApplicationDbContext context,
            UserManager<GlazAccount> userManager,
            IWebHostEnvironment webHostEnvironment,
            IVuforiaService vuforiaService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _vuforiaService = vuforiaService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Account)
                .Include(o => o.Attachments)
                    .ThenInclude(a => a.VuforiaDetails)
                .OrderBy(o => o)
                .ToArrayAsync();

            var clientOrders = orders.Select(order => new ModeratorOrder(order)).ToArray();

            return View(clientOrders);
        }

        public async Task<IActionResult> WriteNote(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Orders.Where(o => o.Id == id)
                .Select(o => o.ModeratorComment)
                .FirstOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            return View(new WriteNoteModel(id, comment));
        }

        [HttpPost]
        public async Task<IActionResult> WriteNote(WriteNoteModel model)
        {
            var order = await _context.Orders.FindAsync(model.Id);
            //TODO: сделать проверку на всякий случай когда сломают.
            order.ModeratorComment = model.Comment;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangeStateAsync(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.Orders.Where(o => o.Id == id)
                .Select(o => o.State)
                .FirstOrDefaultAsync();

            return View(new ChangeStateModel(id, state));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeState(ChangeStateModel model)
        {
            var order = await _context.Orders.FindAsync(model.Id);
            //TODO: сделать проверку на всякий случай когда сломают.
            order.State = model.State;
            _context.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UploadBundles()
        {
            return View();
        }
        public async Task<IActionResult> ViewOrder(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.Attachments)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            return View(order);
        }
    }
}