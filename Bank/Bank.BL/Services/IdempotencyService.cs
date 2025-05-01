using Bank.DTO.DTOs.ServiceBusDto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.BL.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ConcurrentDictionary<string, baseResponse> _cache = new();
        private readonly ConcurrentDictionary<string, string> _httpCache = new();

        public Task<bool> IsRequestProcessed(string idempotencyKey)
        {
            return Task.FromResult(_cache.ContainsKey(idempotencyKey));
        }

        public Task<baseResponse> GetCachedResponse(string idempotencyKey)
        {
            return Task.FromResult(_cache[idempotencyKey]);
        }

        public Task SaveResponse(string idempotencyKey, baseResponse body)
        {
            _cache[idempotencyKey] = body;
            return Task.CompletedTask;
        }
        public Task<bool> HttpIsRequestProcessed(string idempotencyKey)
        {
            return Task.FromResult(_httpCache.ContainsKey(idempotencyKey));
        }

        public Task<string> HttpGetCachedResponse(string idempotencyKey)
        {
            return Task.FromResult(_httpCache[idempotencyKey]);
        }

        public Task HttpSaveResponse(string idempotencyKey, string body)
        {
            _httpCache[idempotencyKey] = body;
            return Task.CompletedTask;
        }
    }
}
