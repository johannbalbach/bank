namespace Bank.BL.Redis.Messages
{
    public interface BaseRedisMessage
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
