using Bank.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class GetBankAccountHistoryCommandDto 
    {
        public BankAccountOperationType OperationType { get; set; }
        public decimal OperatingMoney { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public DateTime OperationDateTime { get; set; }
        public BankAccountOperationInitiator OperationInitiator { get; set; }
        public BankAccountOperationStatus OperationStatus { get; set; }
        public Guid? UserId { get; set; }
        public Guid BankAccountId { get; set; }
    }
    public class GetBankAccountHistoryCommand: baseResponse
    {
        public List<GetBankAccountHistoryCommandDto> getBankAccountHistoryCommandDtos { get; set; } = new List<GetBankAccountHistoryCommandDto>();
    }
}
