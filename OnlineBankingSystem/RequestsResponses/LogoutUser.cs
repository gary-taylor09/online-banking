namespace OnlineBankingSystem.RequestsResponses
{
    public class LogoutUserRequest : BaseRequest
    {
        public string UserName { get; set; }
    }

    public class LogoutUserResponse : BaseResponse
    {
        public string UserName { get; set; }
    }
}
