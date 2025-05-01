using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class baseMessage
    {
        public string IdempotencyKey { get; set; }

        public baseMessage() 
        {
            GenerateIdempotencyKey();
        }
        public void GenerateIdempotencyKey()
        {
            IdempotencyKey = Guid.NewGuid().ToString();
        }
    }
}
