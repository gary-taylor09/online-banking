using OnlineBankingSystem.Database;
using Microsoft.EntityFrameworkCore;

namespace OnlineBankingSystem.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            dbContext.Database.Migrate();
        }

    }
}
