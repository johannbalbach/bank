namespace Core.BL.CQRS.Commands.CloseCardBankAccount
{
    public class CloseCardBankAccountRequest
    {
        public Guid AccountId { get; set; }
    }
}
