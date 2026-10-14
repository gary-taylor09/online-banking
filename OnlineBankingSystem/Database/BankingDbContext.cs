using Microsoft.EntityFrameworkCore;
using OnlineBakingSystem.Database;

namespace OnlineBankingSystem.Database
{
    public class BankingDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }

        public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AccountType>(b =>
            {
                b.ToTable("AccountTypes", "banking");
                b.HasKey(at => at.ID);
                b.Property(at => at.Name).HasMaxLength(50);
            });

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
                b.Property(a => a.Balance).HasColumnType("decimal(18,2)");
                b.Property(a => a.UserId).IsRequired().HasColumnType("nvarchar(450)");
                b.Property(a => a.Deleted).HasDefaultValue(false);
                b.HasOne(a => a.AccountType)
                 .WithMany()
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Transaction>(b =>
            {
                b.ToTable("Transactions", "banking");
                b.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                b.Property(t => t.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                b.Property(t => t.Deleted).HasDefaultValue(false);
                b.HasOne<Account>()
                 .WithMany(a => a.Transactions)
                 .HasForeignKey(t => t.AccountId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
