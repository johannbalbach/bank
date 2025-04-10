using Core.DTO.DTOs.Requests.BankAccount;

namespace Core.BL.CQRS.Commands.PutMoneyOnBankAccount
{
    public class PutMoneyOnCardBankAccountRequest
    {
        public BankAccountMoneyOperationRequestDTO RequestDTO { get; set; } = null!;
        public Guid AccountId { get; set; }
        public bool IsTransfer { get; set; } = false;
    }
}
