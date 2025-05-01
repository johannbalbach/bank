namespace Bank.BL.Redis.Messages
{
    public class ConsumerMessage : BaseRedisMessage
    {
        public string ConsumerName { get; set; }
        public string MessageName { get; set; }
        public int StatusCode { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Message { get; set; }
    }
}
