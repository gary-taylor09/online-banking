using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineBakingSystem.Database
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
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

            builder.Entity<TransactionType>(b =>
            {
                b.ToTable("TransactionTypes", "banking");
                b.HasKey(tt => tt.ID);
                b.Property(tt => tt.Name).HasMaxLength(50);
                b.HasOne<Transaction>()
                 .WithOne()
                 .HasForeignKey<Transaction>(t => t.TransactionTypeID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Account>(b => 
            {
                b.ToTable("Accounts", "banking");
                b.Property(a => a.AccountNumber).HasMaxLength(25).IsRequired();
                b.HasIndex(a => a.AccountNumber).IsUnique();
                b.HasOne<User>()
                 .WithMany(u => u.Accounts)
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Transaction>(b =>
            {
                b.ToTable("Transactions", "banking");
                b.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                b.Property(t => t.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                b.HasOne<Account>()
                 .WithMany(a => a.Transactions)
                 .HasForeignKey(t => t.AccountId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
