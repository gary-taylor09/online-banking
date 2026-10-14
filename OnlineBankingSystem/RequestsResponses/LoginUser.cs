namespace OnlineBankingSystem.RequestsResponses
{
    public class LoginUserRequest : BaseRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class LoginUserResponse : BaseResponse
    {
        public string AccessToken { get; set; }
        public string UserName { get; set; }
    }
}
