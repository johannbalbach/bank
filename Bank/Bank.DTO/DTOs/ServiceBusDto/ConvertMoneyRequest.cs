namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class ConvertMoneyRequest: baseMessage
    {
        public string RequestCurrencyName { get; set; }
        public string AccountCurrencyName { get; set; }
        public decimal RequestCurrencyAmount { get; set; }
    }
}
