using Bank.DAL.Enums;

namespace Core.DTO.DTOs.Requests.BankAccount
{
    public class AllBankAccountsQueryDTO
    {
        public string? FullName { get; set; }
        public BankAccountType? BankAccountType { get; set; }
        public bool? IsFrozen { get; set; }
        public bool? IsClosed { get; set; }
        public int page { get; set; } = 1;
    }
}
