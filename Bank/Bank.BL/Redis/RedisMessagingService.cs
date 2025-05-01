using Bank.BL.Configuration;
using Bank.BL.Redis.Messages;
using StackExchange.Redis;
using System.Text.Json;

namespace Bank.BL.Redis
{
    public class RedisMessagingService : IRedisMessagingService
    {
        private readonly IDatabase _redisDatabase;

        public RedisMessagingService(IRedisDatabaseProvider databaseProvider)
        {
            _redisDatabase = databaseProvider.GetDatabase();
        }

        public async Task<Guid> SetMessageAsync<T>(T message, Guid? messageId = null)
        {
            messageId = messageId == null ? Guid.NewGuid() : messageId.Value;

            var serializedModel = JsonSerializer.Serialize(message);

            _ = await _redisDatabase.StringSetAsync(messageId.ToString(), serializedModel, expiry: TimeSpan.FromMinutes(1), when: When.Always);

            return messageId.Value;
        }
        
        public async Task<T> GetMessageAsync<T>(Guid messageId)
            where T : new()
        {
            string? message = await _redisDatabase.StringGetAsync(messageId.ToString());

            T? redisMessage;

            if(message == null)
            {
                redisMessage = new();
            }
            else
            {
                redisMessage = JsonSerializer.Deserialize<T>(message);
                redisMessage ??= new();
            }

            return redisMessage;
        }

        public async Task PushMessageAsync<T>(T message)
        {
            var stringMessage = JsonSerializer.Serialize(message);

            var listLong = await _redisDatabase.ListLeftPushAsync(RedisConfiguration.MessageQueue, stringMessage);
        }

        public async Task<string?> PopMessageAsync()
        {
            var queueMessage = await _redisDatabase.ListRightPopAsync(RedisConfiguration.MessageQueue);

            string? redisMessage = null;

            if (!queueMessage.IsNullOrEmpty)
            {
                redisMessage = queueMessage.ToString();
            }

            return redisMessage;
        }
    }
}
