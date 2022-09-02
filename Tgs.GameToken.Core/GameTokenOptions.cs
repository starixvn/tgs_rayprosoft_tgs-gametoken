using StackExchange.Redis;

namespace Tgs.GameToken;

public class GameTokenOptions
{
	public Func<IServiceProvider, IDatabase> RedisDatabaseResolver { get; set; } = default!;
}
