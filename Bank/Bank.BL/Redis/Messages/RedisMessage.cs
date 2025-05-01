namespace Bank.BL.Redis.Messages
{
    public class RedisMessage : BaseRedisMessage
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
