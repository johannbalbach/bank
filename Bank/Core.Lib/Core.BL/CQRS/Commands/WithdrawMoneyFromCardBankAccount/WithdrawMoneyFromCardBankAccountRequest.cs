using Core.DTO.DTOs.Requests.BankAccount;

namespace Core.BL.CQRS.Commands.WithdrawMoneyFromCardBankAccount
{
    public class WithdrawMoneyFromCardBankAccountRequest
    {
        public BankAccountMoneyOperationRequestDTO RequestDTO { get; set; } = null!;
        public Guid AccountId { get; set; }
        public bool IsTransfer { get; set; } = false;
    }
}
