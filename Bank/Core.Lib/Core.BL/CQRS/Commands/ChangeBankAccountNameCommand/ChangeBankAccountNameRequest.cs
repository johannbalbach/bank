namespace Core.BL.CQRS.Commands.ChangeBankAccountNameCommand
{
    public class ChangeBankAccountNameRequest
    {
        public string AccountName { get; set; }
        public Guid AccountId { get; set; }
    }
}
