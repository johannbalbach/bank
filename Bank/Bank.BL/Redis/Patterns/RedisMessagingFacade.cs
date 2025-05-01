using Bank.BL.Redis.Messages;

namespace Bank.BL.Redis.Patterns
{
    public class RedisMessagingFacade : IRedisMessagingFacade
    {
        private readonly IRedisMessagingService _redisMessagingService;

        public RedisMessagingFacade(IRedisMessagingService redisMessagingService)
        {
            _redisMessagingService = redisMessagingService;
        }

        public async Task ProcessRedisMessage<T>(Guid messageId, string message, int statusCode)
            where T : BaseRedisMessage, new()
        {
            var redisMessage = await _redisMessagingService.GetMessageAsync<T>(messageId);

            redisMessage.StatusCode = statusCode;
            redisMessage.Message = message;

            _ = await _redisMessagingService.SetMessageAsync(redisMessage, messageId);
        }
    }
}
