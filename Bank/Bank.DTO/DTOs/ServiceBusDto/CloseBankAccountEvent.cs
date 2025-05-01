using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class CloseBankAccountEvent: baseMessage
    {
        public Guid Id { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime CloseDateTime { get; set; }
    }
}
