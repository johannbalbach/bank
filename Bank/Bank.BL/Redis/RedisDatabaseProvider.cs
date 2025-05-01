using StackExchange.Redis;

namespace Bank.BL.Redis
{
    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisDatabaseProvider(ConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }
    }
}
