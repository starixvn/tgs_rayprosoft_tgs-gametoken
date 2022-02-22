using MongoDB.Driver;
using Tgs.GameToken.Configuration;
using Tgs.GameToken.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class GameTokenConfigurationExtensions
{
	public static GameTokenConfiguration SetMongoDbTokenStore(
		this GameTokenConfiguration configuration,
		Func<IServiceProvider, IMongoDatabase> mongoDatabaseResolver)
		=> configuration.SetTokenStore<MongoTokenStore>(sp => new[] { mongoDatabaseResolver(sp) });
}
