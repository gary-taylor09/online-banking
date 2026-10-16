using System.ComponentModel.DataAnnotations;

namespace OnlineBankingSystem.Database
{
    public class Transaction
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public int TransactionTypeID { get; set; }
        [Required]
        public TransactionType Type { get; set; }
        [Required]
        public bool Deleted { get; set; } = false;
    }
}
