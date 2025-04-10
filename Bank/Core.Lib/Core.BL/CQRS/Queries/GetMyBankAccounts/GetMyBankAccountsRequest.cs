namespace Core.BL.CQRS.Queries.GetMyBankAccounts
{
    public class GetMyBankAccountsRequest
    {
        public List<Guid> AccountsIds { get; set; }
    }
}
