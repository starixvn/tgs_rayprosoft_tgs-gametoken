using Microsoft.Extensions.Caching.Memory;

namespace Tgs.GameToken;

internal class MemoryCachedTokenStore(
	ITokenStore sourceTokenStore,
	TimeSpan expiry,
	IMemoryCache memoryCache) : ITokenStore
{
	public ValueTask CreateTokenAsync(Token token, CancellationToken cancellationToken = default)
		=> sourceTokenStore.CreateTokenAsync(token, cancellationToken);

	public async ValueTask<GameTokenInfo?> GetTokenAsync(string id, CancellationToken cancellationToken = default)
	{
		var key = GetKey(id);

		var tokenInfo = await memoryCache.GetOrCreateAsync(key, async cacheEntry =>
		{
			var token = await sourceTokenStore.GetTokenAsync(id).ConfigureAwait(false);

			_ = cacheEntry.SetAbsoluteExpiration(expiry);

			return token;
		}).ConfigureAwait(false);

		if (tokenInfo is not null && tokenInfo.IsValid)
			_ = await sourceTokenStore.GetTokenAsync(
				id: id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

		return tokenInfo;
	}

	public async ValueTask DeleteTokenAsync(string id, CancellationToken cancellationToken = default)
	{
		var key = GetKey(id);

		await sourceTokenStore.DeleteTokenAsync(
			id: id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		memoryCache.Remove(key);
	}

	public ValueTask InitializationAsync(CancellationToken cancellationToken = default) => sourceTokenStore.InitializationAsync(cancellationToken);

	private static string GetKey(string id)
		=> string.Concat("Tgs:GameToken:", id);
}
