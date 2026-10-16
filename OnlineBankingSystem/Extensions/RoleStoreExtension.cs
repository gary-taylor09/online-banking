using Microsoft.AspNetCore.Identity;

namespace OnlineBankingSystem.Extensions
{
    public static class RoleStoreExtension
    {
        private static readonly string[] Roles = { "Admin", "Clerk" , "AccountManager", "AccountViewer" };
        public static async Task SeedRolesAsync(this WebApplication app) 
        {
            using var scoper = app.Services.CreateScope();
            var roleManager = scoper.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
