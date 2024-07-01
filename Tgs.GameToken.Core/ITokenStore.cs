namespace Tgs.GameToken;

public interface ITokenStore
{
	ValueTask CreateTokenAsync(Token token, CancellationToken cancellationToken = default);

	ValueTask<GameTokenInfo?> GetTokenAsync(string id, CancellationToken cancellationToken = default);

	ValueTask DeleteTokenAsync(string id, CancellationToken cancellationToken = default);

	ValueTask InitializationAsync(CancellationToken cancellationToken = default);
}
