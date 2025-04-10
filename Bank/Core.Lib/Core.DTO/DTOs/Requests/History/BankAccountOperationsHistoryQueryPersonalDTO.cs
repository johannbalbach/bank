using Bank.DAL.Enums;

namespace Core.DTO.DTOs.Requests.History
{
    public class BankAccountOperationsHistoryQueryPersonalDTO
    {
        public ICollection<Guid>? BankAccountsIds { get; set; } = null!;
        public DateTime? StartPeriod { get; set; } = null!;
        public DateTime? EndPeriod { get; set; } = null!;
        public BankAccountOperationInitiator? BankAccountOperationInitiator { get; set; }
        public BankAccountOperationStatus? BankAccountOperationStatus { get; set; }
        public BankAccountOperationType? BankAccountOperationType { get; set; }
        public int page { get; set; } = 1;
    }
}
