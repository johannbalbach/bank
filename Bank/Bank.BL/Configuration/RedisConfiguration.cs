using Bank.BL.Redis.Messages;

namespace Bank.BL.Configuration
{
    public class RedisConfiguration
    {
        public const string Redis = "Redis";
        public const string MessageQueue = $"queue_redis";
        public const string RedisWatch = "RedisWatch";

        public int BatchSize { get; set; }
    }
}
