using Bank.DAL.Enums;

namespace CreditService.Db.Entities
{
    public class CreditBankAccount : BankAccount
    {
        public string AccountNumber { get; set; }
        public decimal Debt {  get; set; }
        public Guid TariffId { get; set; }
        public CreditTariff Tariff { get; set; }
        public Guid PayingCardId { get; set; }
        public Guid CreditCardId { get; set;}
    }
}
