using Glaz.Server.Entities;
using Microsoft.AspNetCore.Identity;

namespace Glaz.Server.Data
{
    public static class DatabaseInitializer
    {
        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (roleManager.FindByNameAsync("Admin").Result is null)
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
            }

            if (roleManager.FindByNameAsync("Moderator").Result is null)
            {
                roleManager.CreateAsync(new IdentityRole("Moderator")).Wait();
            }
        }
        public static void SeedUserAccounts(UserManager<GlazAccount> userManager)
        {
            if (userManager.FindByEmailAsync("admin@glaz.ru").Result is null)
            {
                var user = new GlazAccount
                {
                    UserName = "admin",
                    Email = "admin@glaz.ru",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(user, "123456789").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }

            if (userManager.FindByEmailAsync("moderator@glaz.ru").Result is null)
            {
                var user = new GlazAccount
                {
                    UserName = "moderator",
                    Email = "moderator@glaz.ru",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(user, "123456789").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Moderator").Wait();
                }
            }
        }
    }
}