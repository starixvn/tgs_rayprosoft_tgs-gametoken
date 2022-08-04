namespace Tgs.GameToken.Client;

public interface IGameTokenClient
{
	ValueTask<bool> CheckTokenValidAsync(string token, CancellationToken cancellationToken);

	ValueTask<string> CreateTokenAsync(string playerCode, string info, CancellationToken cancellationToken);

	ValueTask<GameTokenInfo?> GetTokenInfoAsync(string token, CancellationToken cancellationToken);

	ValueTask RevokeTokenAsync(string token, CancellationToken cancellationToken);
}
