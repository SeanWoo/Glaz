using Glaz.Server.Data.Enums;
using Glaz.Server.Entities;
using Microsoft.AspNetCore.Identity;

namespace Glaz.Server.Data
{
    public sealed class DatabaseInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<GlazAccount> _userManager;
        
        public DatabaseInitializer(RoleManager<IdentityRole> roleManager, UserManager<GlazAccount> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        
        public void SeedRoles()
        {
            CreateRoleIfNotExists(Roles.Admin);
            CreateRoleIfNotExists(Roles.Moderator);
            CreateRoleIfNotExists(Roles.Customer);
        }
        private void CreateRoleIfNotExists(string roleName)
        {
            if (IsRoleNotExists(roleName))
            {
                CreateNewRole(roleName);
            }
        }
        private bool IsRoleNotExists(string roleName)
        {
            return _roleManager.FindByNameAsync(roleName).Result is null;
        }
        private void CreateNewRole(string roleName)
        {
            _roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
        }
        
        public void SeedUserAccounts()
        {
            CreateAccountWithRoleIfNotExists("admin", Roles.Admin);
            CreateAccountWithRoleIfNotExists("moder", Roles.Moderator);
            CreateAccountWithRoleIfNotExists("user", Roles.Customer);
        }
        private void CreateAccountWithRoleIfNotExists(string username, string role)
        {
            if (IsAccountNotExistsByUsername(username))
            {
                CreateAccountWithRole(username, role);
            }
        }
        private bool IsAccountNotExistsByUsername(string username)
        {
            return _userManager.FindByNameAsync(username).Result is null;
        }
        private void CreateAccountWithRole(string username, string role)
        {
            var user = new GlazAccount
            {
                UserName = username,
                Email = $"{username}@glaz.ru",
                EmailConfirmed = true
            };

            var result = _userManager.CreateAsync(user, "123456789").Result;
            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(user, role).Wait();
            }
        }
    }
}