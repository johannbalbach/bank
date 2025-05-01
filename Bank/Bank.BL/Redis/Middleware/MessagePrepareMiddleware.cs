using Bank.BL.Configuration;
using Bank.BL.Redis.Messages;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Bank.BL.Redis.Middleware
{
    public class MessagePrepareMiddleware
    {
        private readonly RequestDelegate _next;

        public MessagePrepareMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IRedisMessagingService redisMessagingService)
        {
            Stopwatch watch = Stopwatch.StartNew();

            string httpMethod = httpContext.Request.Method;
            string path = httpContext.Request.Path;
            DateTime now = DateTime.UtcNow;

            var messageId = await redisMessagingService.SetMessageAsync(new RedisMessage
            {
                Method = httpMethod,
                Path = path,
                Timestamp = now
            });

            httpContext.Items[RedisConfiguration.RedisWatch] = messageId.ToString();

            await _next(httpContext);

            watch.Stop();

            var redisMessage = await redisMessagingService.GetMessageAsync<RedisMessage>(messageId);

            redisMessage.StatusCode = httpContext.Response.StatusCode;
            redisMessage.ElapsedMilliseconds = watch.ElapsedMilliseconds;

            await redisMessagingService.PushMessageAsync(redisMessage);
        }
    }
}
