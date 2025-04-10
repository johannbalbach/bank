using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class CreditAccountDto
    {
        public Guid Id { get; set; }
        public bool IsFrozen { get; set; }
        public string CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public decimal Debt { get; set; }
        public string BankAccountType { get; set; }
        public string OwnerId { get; set; }
        public TariffBriefDto Tariff { get; set; }
    }
}
