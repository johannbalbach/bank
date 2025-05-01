using Bank.BL.Redis.Messages;

namespace Bank.BL.Redis
{
    public interface IRedisMessagingService
    {
        public Task<Guid> SetMessageAsync<T>(T message, Guid? messageId = null);
        public Task<T> GetMessageAsync<T>(Guid messageId) where T : new();

        public Task PushMessageAsync<T>(T message);
        public Task<string> PopMessageAsync();
    }
}
