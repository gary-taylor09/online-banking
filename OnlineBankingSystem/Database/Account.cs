using OnlineBankingSystem.Database;
using System.ComponentModel.DataAnnotations;

namespace OnlineBankingSystem.Database
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        // FK to Identity user (IdentityUser.Id is string)
        [Required]
        [StringLength(450)]
        public string UserId { get; set; }
        [Required]
        [StringLength(25)]
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        // transactions on this account
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        [Required]
        public AccountType AccountType { get; set; }
        [Required]
        public bool Deleted { get; set; } = false;
    }
}
