using Glaz.Server.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Glaz.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<GlazAccount>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}