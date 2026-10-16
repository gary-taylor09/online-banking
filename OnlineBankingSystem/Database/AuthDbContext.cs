using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineBankingSystem.Database
{
    public class AuthDbContext : IdentityDbContext<User>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(b =>
            {
                b.Property(u => u.UserName).HasMaxLength(50);
                b.Property(u => u.NormalizedUserName).HasMaxLength(50);
                b.HasIndex(u => u.NormalizedUserName).IsUnique();
            });

            builder.HasDefaultSchema("identity");
        }
    }
}
