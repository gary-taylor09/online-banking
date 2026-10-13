using System.ComponentModel.DataAnnotations;

namespace OnlineBakingSystem.Database
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
