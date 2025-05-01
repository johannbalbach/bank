using Bank.BL.Redis.Messages;

namespace Bank.BL.Redis.Patterns
{
    public interface IRedisMessagingFacade
    {
        public Task ProcessRedisMessage<T>(Guid messageId, string message, int statusCode) where T : BaseRedisMessage, new();
    }
}
