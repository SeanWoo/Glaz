using Glaz.Server.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Glaz.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<GlazAccount>
    {
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<VuforiaDetails> VuforiaDetails { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}