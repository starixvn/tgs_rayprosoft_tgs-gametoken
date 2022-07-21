namespace Tgs.GameToken.Clients;

public interface IGameTokenClient
{
	ValueTask<bool> CheckTokenValidAsync(string token, CancellationToken cancellationToken);

	ValueTask<string> CreateTokenAsync(string playerCode, CancellationToken cancellationToken);

	ValueTask<GameTokenInfo?> GetTokenInfoAsync(string token, CancellationToken cancellationToken);

	ValueTask RevokeTokenAsync(string token, CancellationToken cancellationToken);
}
