namespace Tgs.GameToken;

internal class NoopTokenStore : ITokenStore
{
	public ValueTask CreateTokenAsync(Token token, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

	public ValueTask<GameTokenInfo?> GetTokenAsync(string id, CancellationToken cancellationToken = default) => ValueTask.FromResult<GameTokenInfo?>(null);

	public ValueTask DeleteTokenAsync(string id, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

	public ValueTask InitializationAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
