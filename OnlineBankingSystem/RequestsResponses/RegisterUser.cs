namespace OnlineBankingSystem.RequestsResponses
{
    public class RegisterUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
    }

    public class RegisterUserResponse : BaseResponse
    {
        public string UserId { get; set; }
    }
}
