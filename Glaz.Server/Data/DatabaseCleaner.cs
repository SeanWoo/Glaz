using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Glaz.Server.Data
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class DatabaseCleaner
    {
        private readonly ILogger<DatabaseCleaner> _logger;
        private readonly ApplicationDbContext _context;

        public DatabaseCleaner(ILogger<DatabaseCleaner> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task RemoveMarkedOrderEntities()
        {
            var orders = await _context.Orders
                .Include(o => o.Attachments)
                .ThenInclude(a => a.VuforiaDetails)
                .Where(o => o.HastToDelete)
                .ToListAsync();

            if (orders.Count != 0)
            {
                _context.RemoveRange(orders);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"{DateTime.Now} — {orders.Count} Orders have been deleted.");
        }
    }
}