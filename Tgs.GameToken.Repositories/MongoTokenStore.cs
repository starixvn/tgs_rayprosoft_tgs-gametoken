using MongoDB.Driver;

namespace Tgs.GameToken.Repositories;

internal class MongoTokenStore : ITokenStore
{
	private readonly ITokenStore _sourceTokeStore;
	private readonly IMongoDatabase _mongoDatabase;
	private readonly IMongoCollection<MongoToken> _tokenCollection;

	public MongoTokenStore(ITokenStore sourceTokeStore, IMongoDatabase mongoDatabase)
	{
		_sourceTokeStore = sourceTokeStore ?? throw new ArgumentNullException(nameof(sourceTokeStore));
		_mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
		_tokenCollection = _mongoDatabase.GetCollection<MongoToken>("GameTokens");
	}

	public async ValueTask CreateTokenAsync(Token token)
	{
		await _tokenCollection
			.InsertOneAsync(new MongoToken
			{
				Id = token.Id,
				PlayerCode = token.PlayerCode,
				CreatedOn = token.CreatedOn
			})
			.ConfigureAwait(false);

		await _sourceTokeStore
			.CreateTokenAsync(token)
			.ConfigureAwait(false);
	}

	public async ValueTask<GameTokenInfo?> GetTokenAsync(string id)
	{
		var tokenData = await _tokenCollection
			.Find(row => row.Id == id)
			.FirstOrDefaultAsync().ConfigureAwait(false);

		return tokenData is null
			? null
			: new GameTokenInfo(
				tokenData.Id,
				tokenData.PlayerCode,
				tokenData.CreatedOn,
				false);
	}

	public ValueTask DeleteTokenAsync(string id) => _sourceTokeStore.DeleteTokenAsync(id);
}
