namespace OnlineBankingSystem.RequestsResponses
{
    public class CreateTransactionRequest : BaseRequest
    {
        public int AccountId { get; set; }
        public string TransactionType { get; set; } // "Withdrawal", "Deposit", "Transfer"
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int? TransferToAccountId { get; set; } // Optional, for transfer transactions
    }

    public class CreateTransactionResponse : BaseResponse
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal NewAccountBalance { get; set; }
    }
}
