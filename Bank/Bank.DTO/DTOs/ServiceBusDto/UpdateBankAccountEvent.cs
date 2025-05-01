using Bank.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class UpdateBankAccountEvent: baseMessage
    {
        public Guid Id { get; set; }
        public decimal Debt { get; set; }
        public decimal Balance { get; set; }
    }
}
