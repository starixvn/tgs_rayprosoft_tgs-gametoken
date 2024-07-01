using MongoDB.Driver;
using Tgs.GameToken.Repositories.Extensions;

namespace Tgs.GameToken.Repositories;

internal class MongoTokenStore(
	ITokenStore sourceTokeStore,
	IMongoDatabase mongoDatabase) : ITokenStore
{
	private readonly ITokenStore _sourceTokeStore = sourceTokeStore ?? throw new ArgumentNullException(nameof(sourceTokeStore));
	private readonly IMongoCollection<Models.GameToken> _tokenCollection = mongoDatabase.GetCollection<Models.GameToken>(typeof(Models.GameToken).GetTableName());

	public async ValueTask CreateTokenAsync(Token token, CancellationToken cancellationToken = default)
	{
		await _tokenCollection.InsertOneAsync(
			document: new Models.GameToken
			{
				Id = token.Id,
				PlayerCode = token.PlayerCode,
				CreatedOn = token.CreatedOn,
				ExpireOn = DateTime.UtcNow.AddDays(+180)
			},
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<GameTokenInfo?> GetTokenAsync(string id, CancellationToken cancellationToken = default)
	{
		var tokenData = await _tokenCollection
			.Find(row => row.Id == id)
			.FirstOrDefaultAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return tokenData is null
			? null
			: new GameTokenInfo(
				tokenData.Id,
				tokenData.PlayerCode,
				tokenData.CreatedOn,
				false);
	}

	public ValueTask DeleteTokenAsync(string id, CancellationToken cancellationToken = default) => _sourceTokeStore.DeleteTokenAsync(id, cancellationToken);

	public async ValueTask InitializationAsync(CancellationToken cancellationToken = default)
	{
		await _tokenCollection.Indexes.CreateOneAsync(
			model: new CreateIndexModel<Models.GameToken>(
				keys: Builders<Models.GameToken>.IndexKeys
				.Ascending(m => m.ExpireOn),
				options: new CreateIndexOptions
				{
					ExpireAfter = TimeSpan.FromSeconds(1),
					Name = "idx_expire_on"
				}),
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}
