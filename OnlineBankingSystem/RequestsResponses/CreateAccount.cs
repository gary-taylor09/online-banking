using OnlineBankingSystem.Database;

namespace OnlineBankingSystem.RequestsResponses
{
    public class CreateAccountRequest : BaseRequest
    {
        public string AccountHolderUserId { get; set; }
        public AccountType AccountType { get; internal set; }
        public decimal InitialDeposit { get; set; }
    }

    public class CreateAccountResponse : BaseResponse
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
    }
}
