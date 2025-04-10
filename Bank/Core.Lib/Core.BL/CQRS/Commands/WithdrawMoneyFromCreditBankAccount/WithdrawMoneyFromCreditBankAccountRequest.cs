using Core.DTO.DTOs.Requests.BankAccount;

namespace Core.BL.CQRS.Commands.WithdrawMoneyFromCreditBankAccount
{
    public class WithdrawMoneyFromCreditBankAccountRequest
    {
        public BankAccountMoneyOperationRequestDTO RequestDTO { get; set; }
        public Guid AccountId { get; set; }
        public bool IsTransfer { get; set; } = false;
    }
}
