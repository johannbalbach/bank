using Bank.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class CreateBankAccountEvent
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Debt { get; set; }
        public Guid TariffId { get; set; }
        public string CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public BankAccountType AccountType { get; set; }
        public Guid OwnerId { get; set; }
        public string AccountName { get; set; }
        public bool IsFrozen { get; set; }
    }
}
