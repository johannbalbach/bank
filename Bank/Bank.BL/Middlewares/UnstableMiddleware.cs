using Bank.BL.Configuration;
using Bank.BL.Redis;
using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.BL.Middlewares
{
    public class UnstableMiddleware
    {
        private readonly RequestDelegate _next;

        public UnstableMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRedisMessagingFacade redisMessagingFacade)
        {
            var currentMinute = DateTime.UtcNow.Minute;
            var isEvenMinute = currentMinute % 2 == 0;
            var errorProbability = isEvenMinute ? 0.9 : 0.5;

            var random = new Random();
            if (random.NextDouble() < errorProbability)
            {
                Guid id = Guid.Parse(context.Items[RedisConfiguration.RedisWatch] as string);
                await redisMessagingFacade.ProcessRedisMessage<RedisMessage>(id, "Simulated Server Error", 503);

                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Simulated Server Error");
                return;
            }

            await _next(context);
        }
    }
}
