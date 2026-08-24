using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Data
{
    public class DbSeeder
    {
        private readonly string _adminUsername;
        private readonly string _adminPassword;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DbSeeder(IConfiguration config, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _adminUsername = config["AdminUsername"]
                ?? throw new InvalidOperationException("Admin username not configured");
            _adminPassword = config["AdminPassword"]
                ?? throw new InvalidOperationException("Admin password not configured");
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync(_roleManager);
            await SeedAdminUserAsync(_userManager);
        }



        public async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { Roles.Admin, Roles.User };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var createRole = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!createRole.Succeeded)
                    {
                        var errorMessages = string.Join(", ", createRole.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed added role {roleName}: {errorMessages}");
                    }
                }
            }
        }

        public async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var nameExists = await userManager.FindByNameAsync(_adminUsername);
            if (nameExists != null)
            {
                var userRole = await userManager.GetRolesAsync(nameExists);
                if (userRole.Contains(Roles.Admin))
                {
                    return;
                }
                throw new InvalidOperationException($"Admin username '{_adminUsername}' already exists but he has not role admin.");
            }
            try
            {
                var adminUser = new ApplicationUser
                {
                    UserName = _adminUsername,
                };

                var newAdmin = await userManager.CreateAsync(adminUser, _adminPassword);
                if (!newAdmin.Succeeded)
                {
                    var errorMessages = string.Join(", ", newAdmin.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errorMessages}");
                }
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                if (!addToRoleResult.Succeeded)
                {
                    var errorMessages = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to assign admin role: {errorMessages}");
                }
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create admin user: {ex.Message}", ex);
            }
        }
    }
}
