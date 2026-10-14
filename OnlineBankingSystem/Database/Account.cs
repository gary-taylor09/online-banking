using System.ComponentModel.DataAnnotations;

namespace OnlineBakingSystem.Database
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        // FK to Identity user (IdentityUser.Id is string)
        [Required]
        public string UserId { get; set; }
        // navigation to Identity user
        public User Owner { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        // transactions on this account
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
