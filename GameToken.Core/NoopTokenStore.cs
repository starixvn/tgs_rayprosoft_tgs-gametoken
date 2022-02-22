namespace Tgs.GameToken;

internal class NoopTokenStore : ITokenStore
{
	public ValueTask CreateTokenAsync(Token token) => ValueTask.CompletedTask;

	public ValueTask<GameTokenInfo?> GetTokenAsync(string id) => ValueTask.FromResult<GameTokenInfo?>(null);
}
