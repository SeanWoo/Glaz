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
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Target)
                .WithMany(a => a.TargetOrders)
                .HasForeignKey(o => o.TargetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.ResponseFile)
                .WithMany(a => a.ResponseOrders)
                .HasForeignKey(o => o.ResponseFileId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}