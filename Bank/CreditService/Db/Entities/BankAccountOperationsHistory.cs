using Bank.DAL.Enums;

namespace CreditService.Db.Entities
{
    public class BankAccountOperationsHistory : BaseDeletableEntity
    {
        public BankAccountOperationType OperationType { get; set; }
        public decimal OperatingMoney { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public DateTime OperationDateTime { get; set; }
        public BankAccountOperationInitiator OperationInitiator { get; set; }
        public BankAccountOperationStatus OperationStatus { get; set; }
        public Guid? UserId { get; set; }
        public Guid BankAccountId { get; set; }

    }
}
