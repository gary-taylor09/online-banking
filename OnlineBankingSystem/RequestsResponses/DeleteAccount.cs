namespace OnlineBankingSystem.RequestsResponses
{
    public class DeleteAccountRequest : BaseRequest
    {
        public int AccountId { get; set; }
    }

    public class DeleteAccountResponse : BaseResponse
    {
        public int AccountId { get; set; }
    }
}
