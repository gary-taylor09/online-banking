namespace OnlineBankingSystem.RequestsResponses
{
    public class ReconciliationRequest : BaseRequest
    {
        public int TransactionId { get; set; }
        public decimal NewAmount { get; set; }
    }

    public class ReconciliationResponse : BaseResponse
    {
        public int TransactionId { get; set; }
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public decimal AccountBalanceAdjustment { get; set; }
    }
}
