namespace Core.BL.CQRS.Commands.TransferMoneyBetweenBankAccounts
{
    public class TransferMoneyBetweenBankAccountsRequest
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }

        public string CurrencyType { get; set; }
        public decimal Amount { get; set; }
    }
}
