using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class IsUserExistCommand: baseResponse
    {
        public bool IsExist { get; set; }
    }
}
