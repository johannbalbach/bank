using Bank.DAL.Enums;

namespace CreditService.Db.Entities
{
    public abstract class BankAccount : BaseDeletableEntity
    {
        public DateTime? CloseDateTime { get; set; }
        public string CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public BankAccountType AccountType { get; set; }
        public Guid OwnerId { get; set; }
        public string AccountName { get; set; }
        public bool IsFrozen { get; set; }
    }
}
