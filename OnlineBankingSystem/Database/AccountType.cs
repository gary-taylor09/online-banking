using System.ComponentModel.DataAnnotations;

namespace OnlineBankingSystem.Database
{
    public class AccountType
    {
        [Key]
        [Required]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
