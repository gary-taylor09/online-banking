using Microsoft.AspNetCore.Identity;

namespace FakeBankingSystem.Database
{
    public class User : IdentityUser
    {
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
