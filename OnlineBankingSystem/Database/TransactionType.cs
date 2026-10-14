using System.ComponentModel.DataAnnotations;

namespace OnlineBakingSystem.Database
{
    public class TransactionType 
    { 
        [Key]  
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
