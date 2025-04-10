using Core.DAL.Models.History;
using Core.DTO.DTOs.Responses.History;

namespace Core.BL.Extensions
{
    public static class BankAccountOperationsHistoryExtensions
    {
        public static BankAccountOperationsHistoryResponseDTO BankAccountOperationsHistoryToDTO(this BankAccountOperationsHistory history)
        {
            return new BankAccountOperationsHistoryResponseDTO
            {
                BankAccountOperationType = history.BankAccountOperationType,
                OperatingMoney = history.OperatingMoney,
                CurrentBalance = history.CurrentBalance,
                PreviousBalance = history.PreviousBalance,
                OperationDateTime = history.OperationDateTime,
                BankAccountOperationInitiator = history.BankAccountOperationInitiator,
                BankAccountOperationStatus = history.BankAccountOperationStatus,
                BankAccountId = history.BankAccountId,
                UserId = history.UserId
            };
        }
    }
}
