using Glaz.Server.Entities;
using Glaz.Server.Entities.ManyToMany;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Glaz.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<GlazAccount>
    {
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<OrderState> OrderStates { get; set; }
        public DbSet<Order> Orders { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentToOrder>()
                .HasKey(ato => new {ato.AttachmentId, ato.OrderId});

            modelBuilder.Entity<AttachmentToOrder>()
                .HasOne(ato => ato.Attachment)
                .WithMany(a => a.AttachmentToOrders)
                .HasForeignKey(ato => ato.AttachmentId);

            modelBuilder.Entity<AttachmentToOrder>()
                .HasOne(ato => ato.Order)
                .WithMany(o => o.AttachmentToOrders)
                .HasForeignKey(ato => ato.OrderId);

            base.OnModelCreating(modelBuilder);
        }
    }
}