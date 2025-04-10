using Core.DTO.DTOs.Requests.BankAccount;

namespace Core.BL.CQRS.Commands.CreateNewCardBankAccount
{
    public class CreateNewCardBankAccountRequest
    {
        public DebitCardBankAccountCreateRequestDTO RequestDTO { get; set; }
    }
}
