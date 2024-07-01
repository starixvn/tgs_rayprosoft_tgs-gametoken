using MongoDB.Driver;

namespace Tgs.GameToken;

public class MongoConnectionManager
{
	private readonly MongoClient _mongoClient;

	public IMongoDatabase Database { get; }

	public MongoConnectionManager(string connectionString)
	{
		var mongoUrl = new MongoUrlBuilder(connectionString).ToMongoUrl();

		_mongoClient = new MongoClient(mongoUrl.Url);

		Database = _mongoClient.GetDatabase("game_token_master");
	}
}
