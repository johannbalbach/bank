using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class BankAccountOperationsHistoryListDTO
    {
        public BankAccountOperationType BankAccountOperationType { get; set; }
        public decimal OperatingMoney { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public DateTime OperationDateTime { get; set; }
        public BankAccountOperationInitiator BankAccountOperationInitiator { get; set; }
        public BankAccountOperationStatus BankAccountOperationStatus { get; set; }

        public Guid BankAccountId { get; set; }
        public Guid? UserId { get; set; }
    }
}
