namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class GetBankAccountBalanceRequest: baseMessage
    {
        public Guid BankAccountId { get; set; }
    }
}
