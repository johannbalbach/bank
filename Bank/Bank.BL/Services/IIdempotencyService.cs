using Bank.DTO.DTOs.ServiceBusDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.BL.Services
{
    public interface IIdempotencyService
    {
        Task<bool> IsRequestProcessed(string idempotencyKey);
        Task<baseResponse> GetCachedResponse(string idempotencyKey);
        Task SaveResponse(string idempotencyKey, baseResponse body);
        Task<bool> HttpIsRequestProcessed(string idempotencyKey);
        Task<string> HttpGetCachedResponse(string idempotencyKey);
        Task HttpSaveResponse(string idempotencyKey, string body);
    }
}
