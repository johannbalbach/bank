using Core.DTO.DTOs.Requests.BankAccount;

namespace Core.BL.CQRS.Commands.PutMoneyOnCreditBankAccount
{
    public class PutMoneyOnCreditBankAccountRequest
    {
        public BankAccountMoneyOperationRequestDTO RequestDTO { get; set; }
        public Guid AccountId { get; set; }
        public bool IsTransfer { get; set; } = false;
    }
}
