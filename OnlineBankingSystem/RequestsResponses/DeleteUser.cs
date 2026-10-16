namespace OnlineBankingSystem.RequestsResponses
{
    public class DeleteUserRequest : BaseRequest
    {
        public string UserName { get; set; }
    }

    public class DeleteUserResponse : BaseResponse
    {
        public string UserName { get; set; }
    }
}
