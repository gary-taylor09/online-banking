using OnlineBankingSystem.Database;

namespace OnlineBankingSystem.RequestsResponses
{
    public class GetAccountsForUserRequest : BaseRequest
    {
        public string UserId { get; set; }
    }

    public class GetAccountsForUserResponse : BaseResponse
    {
        public List<Account> Accounts { get; set; }
    }
}
