namespace OnlineBankingSystem.RequestsResponses
{
    internal class GetCurrentUserResponse : BaseResponse
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}