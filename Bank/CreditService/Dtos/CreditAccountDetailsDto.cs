using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class CreditAccountDetailsDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDateTime { get; set; }
        public bool IsFrozen { get; set; }
        public string CurrencyType { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal Debt {  get; set; }
        public string BankAccountType { get; set; }
        public string OwnerId { get; set; }
        public TariffDto Tariff { get; set; }
        public Guid PayingCardId { get; set; }
        public Guid CreditCardId { get; set; }
    }
}
