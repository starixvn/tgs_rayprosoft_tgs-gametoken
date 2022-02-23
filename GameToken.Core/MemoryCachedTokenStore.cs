using Microsoft.Extensions.Caching.Memory;

namespace Tgs.GameToken;

internal class MemoryCachedTokenStore : ITokenStore
{
	private readonly ITokenStore _sourceTokenStore;
	private readonly TimeSpan _expiry;
	private readonly IMemoryCache _memoryCache;

	public MemoryCachedTokenStore(ITokenStore sourceTokenStore, TimeSpan expiry, IMemoryCache memoryCache)
	{
		_sourceTokenStore = sourceTokenStore ?? throw new ArgumentNullException(nameof(sourceTokenStore));
		_expiry = expiry;
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public ValueTask CreateTokenAsync(Token token)
		=> _sourceTokenStore.CreateTokenAsync(token);

	public async ValueTask<GameTokenInfo?> GetTokenAsync(string id)
	{
		var key = $"Tgs:GameToken:{id}";

		var tokenInfo = await _memoryCache.GetOrCreateAsync(key, async cacheEntry =>
		{
			var token = await _sourceTokenStore.GetTokenAsync(id).ConfigureAwait(false);

			if (token == null)
				cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
			else
				cacheEntry.SetSlidingExpiration(_expiry);

			return token;
		}).ConfigureAwait(false);

		if (tokenInfo is not null && tokenInfo.IsValid)
			_ = _sourceTokenStore.GetTokenAsync(id);

		return tokenInfo;
	}

	public async ValueTask DeleteTokenAsync(string id)
	{
		var key = $"Tgs:GameToken:{id}";

		await _sourceTokenStore.DeleteTokenAsync(id).ConfigureAwait(false);

		_memoryCache.Remove(key);
	}
}
