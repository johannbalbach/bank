using Core.DTO.DTOs.Responses.BankAccounts.Tariff;

namespace Core.DTO.DTOs.Responses.BankAccounts
{
    public class CreditBankAccountFullResponseDTO : BankAccountResponseDTO
    {
        public decimal Debt { get; set; }
        public string AccountNumber { get; set; }
        public TariffDTO Tariff { get; set; }
    }
}
