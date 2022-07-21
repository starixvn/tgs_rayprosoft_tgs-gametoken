using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Tgs.GameToken;

internal class RedisTokenStore : ITokenStore
{
	private static readonly JsonSerializerOptions _SerializerOptions = new JsonSerializerOptions
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly ITokenStore _sourceTokenStore;
	private readonly TimeSpan _expiry;
	private readonly IDatabase _redisDatabase;
	private readonly ILogger<RedisTokenStore> _logger;

	public RedisTokenStore(
		ITokenStore sourceTokenStore,
		TimeSpan expiry,
		IDatabase redisDatabase,
		ILogger<RedisTokenStore> logger)
	{
		_sourceTokenStore = sourceTokenStore ?? throw new ArgumentNullException(nameof(sourceTokenStore));
		_expiry = expiry;
		_redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask CreateTokenAsync(Token token)
	{
		var key = $"Tgs:GameToken:{token.Id}";

		var json = JsonSerializer.Serialize(token, _SerializerOptions);

		_ = await _redisDatabase.StringSetAsync(
			key,
			json,
			expiry: _expiry,
			when: When.NotExists).ConfigureAwait(false);

		await _sourceTokenStore.CreateTokenAsync(token).ConfigureAwait(false);
	}

	public async ValueTask<GameTokenInfo?> GetTokenAsync(string id)
	{
		var key = $"Tgs:GameToken:{id}";

		var json = await _redisDatabase.StringGetAsync(key).ConfigureAwait(false);

		if (json.HasValue)
			_ = await _redisDatabase.StringSetAsync(
				key,
				json,
				_expiry).ConfigureAwait(false);
		else
			return await _sourceTokenStore.GetTokenAsync(id).ConfigureAwait(false);

		_logger.LogInformation($"Redis token: {json}");

		var token = JsonSerializer.Deserialize<Token>(json, _SerializerOptions);

		return token is null
			? null
			: new GameTokenInfo(
				token.Id,
				token.PlayerCode,
				token.Info,
				token.CreatedDate,
				true);
	}

	public async ValueTask DeleteTokenAsync(string id)
	{
		await _sourceTokenStore.DeleteTokenAsync(id).ConfigureAwait(false);

		var key = $"Tgs:GameToken:{id}";

		_ = await _redisDatabase.KeyDeleteAsync(key).ConfigureAwait(false);
	}
}
