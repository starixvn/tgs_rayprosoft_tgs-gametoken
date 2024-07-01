using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Tgs.GameToken;

internal class AuthStore(
	ILogger<AuthStore> logger,
	IDatabase database,
	TimeSpan expiry) : IAuthStore
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public async ValueTask CreateAuthAsync(Auth auth, CancellationToken cancellationToken = default)
	{
		var key = GetKey(auth.Id);

		var json = JsonSerializer.Serialize(auth, _SerializerOptions);

		_ = await database
			.StringSetAsync(
			key: key,
			value: json,
			expiry: expiry,
			when: When.NotExists)
			.ConfigureAwait(false);
	}

	public async ValueTask DeleteAuthAsync(string id, CancellationToken cancellationToken = default)
	{
		var key = GetKey(id);

		_ = await database
			.KeyDeleteAsync(
			key: key,
			flags: CommandFlags.FireAndForget)
			.ConfigureAwait(false);
	}

	public async ValueTask<GameAuthInfo?> GetAuthAsync(string id, CancellationToken cancellationToken = default)
	{
		var key = GetKey(id);

		var json = await database
			.StringGetAsync(
			key: key)
			.ConfigureAwait(false);
		logger.LogInformation("Redis token: {json}", json);
		if (!json.HasValue)
		{
			return null;
		}

		var token = JsonSerializer.Deserialize<Auth>(json!, _SerializerOptions);

		return token is null
			? null
			: new GameAuthInfo(
				token.Id,
				token.PlayerCode,
				token.Info);
	}

	private static string GetKey(string id)
		=> $"Tgs:GameToken:Auth:{id}".ToLower();
}
