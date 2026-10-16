namespace OnlineBankingSystem.RequestsResponses
{
    public class UpdateUserPasswordRequest : BaseRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }

    public class UpdateUserPasswordResponse : BaseResponse
    {
        public string UserName { get; set; }
    }
}
