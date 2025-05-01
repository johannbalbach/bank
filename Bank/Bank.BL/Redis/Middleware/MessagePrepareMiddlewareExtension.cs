using Microsoft.AspNetCore.Builder;

namespace Bank.BL.Redis.Middleware
{
    public static class MessagePrepareMiddlewareExtension
    {
        public static void UseRedisMessageMiddleware(this WebApplication application)
        {
            application.UseMiddleware<MessagePrepareMiddleware>();
        }
    }
}
