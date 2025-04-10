namespace Core.BL.CQRS.Queries.GetUserBankAccounts
{
    public class GetUserBankAccountsRequest
    {
        public Guid UserId { get; set; }
        public List<Guid> AccountsIds { get; set; } = [];
    }
}
