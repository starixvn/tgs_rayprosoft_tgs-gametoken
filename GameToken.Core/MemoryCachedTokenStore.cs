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

		return await _memoryCache.GetOrCreateAsync(key, async cacheEntry =>
		{
			var token = await _sourceTokenStore.GetTokenAsync(id).ConfigureAwait(false);

			if (token == null)
				cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
			else
				cacheEntry.SetSlidingExpiration(_expiry);

			return token;
		}).ConfigureAwait(false);
	}
}
