using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Tgs.GameToken;

internal class RedisTokenStore(
	ITokenStore sourceTokenStore,
	TimeSpan expiry,
	IDatabase redisDatabase,
	ILogger<RedisTokenStore> logger) : ITokenStore
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public async ValueTask CreateTokenAsync(Token token, CancellationToken cancellationToken = default)
	{
		var key = GetKey(token.Id);

		var json = JsonSerializer.Serialize(token, _SerializerOptions);

		_ = await redisDatabase.StringSetAsync(
			key,
			json,
			expiry: expiry,
			when: When.NotExists).ConfigureAwait(false);

		await sourceTokenStore.CreateTokenAsync(token, cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<GameTokenInfo?> GetTokenAsync(string id, CancellationToken cancellationToken = default)
	{
		var key = GetKey(id);

		var json = await redisDatabase.StringGetAsync(key).ConfigureAwait(false);

		if (json.HasValue)
			_ = await redisDatabase.StringSetAsync(
				key,
				json,
				expiry).ConfigureAwait(false);
		else
			return await sourceTokenStore.GetTokenAsync(id, cancellationToken).ConfigureAwait(false);

		logger.LogInformation("Redis token: {json}", json);

		var token = JsonSerializer.Deserialize<Token>(json!, _SerializerOptions);

		return token is null
			? null
			: new GameTokenInfo(
				token.Id,
				token.PlayerCode,
				token.CreatedOn,
				true);
	}

	public async ValueTask DeleteTokenAsync(string id, CancellationToken cancellationToken = default)
	{
		var key = GetKey(id);

		await sourceTokenStore.DeleteTokenAsync(
			id: id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		_ = await redisDatabase.KeyDeleteAsync(
			key: key,
			flags: CommandFlags.FireAndForget)
			.ConfigureAwait(false);
	}

	public ValueTask InitializationAsync(CancellationToken cancellationToken = default)
		=> sourceTokenStore.InitializationAsync(cancellationToken);

	private static string GetKey(string id)
		=> string.Concat("Tgs:GameToken:", id);
}
