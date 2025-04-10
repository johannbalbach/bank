using Bank.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class CreateTariffEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MinimumPayment { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
