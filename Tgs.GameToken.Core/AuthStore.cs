using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Tgs.GameToken;

internal class AuthStore : IAuthStore
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly IDatabase _database;
	private readonly TimeSpan _expiry;
	private readonly ILogger<AuthStore> _logger;

	public AuthStore(
		ILogger<AuthStore> logger,
		IDatabase database,
		TimeSpan expiry)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_database = database ?? throw new ArgumentNullException(nameof(database));
		_expiry = expiry;
	}

	public async ValueTask CreateAuthAsync(Auth auth)
	{
		var key = GetKey(auth.Id);

		var json = JsonSerializer.Serialize(auth, _SerializerOptions);

		_ = await _database
			.StringSetAsync(
			key: key,
			value: json,
			expiry: _expiry,
			when: When.NotExists)
			.ConfigureAwait(false);
	}

	public async ValueTask DeleteAuthAsync(string id)
	{
		var key = GetKey(id);

		_ = await _database
			.KeyDeleteAsync(
			key: key,
			flags: CommandFlags.FireAndForget
			)
			.ConfigureAwait(false);
	}

	public async ValueTask<GameAuthInfo?> GetAuthAsync(string id)
	{
		var key = GetKey(id);

		var json = await _database
			.StringGetAsync(
			key: key)
			.ConfigureAwait(false);
		_logger.LogInformation("Redis token: {json}", json);
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
