using StackExchange.Redis;

namespace Tgs.GameToken;

public class RedisConnectionManager
{
	private ConnectionMultiplexer _connection;

	public IDatabase Database { get; }

	public RedisConnectionManager(string connectionString)
	{
		_connection = ConnectionMultiplexer.Connect(connectionString);
		Database = _connection.GetDatabase();
	}
}
