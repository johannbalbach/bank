using StackExchange.Redis;

namespace Bank.BL.Redis
{
    public interface IRedisDatabaseProvider
    {
        public IDatabase GetDatabase();
    }
}
