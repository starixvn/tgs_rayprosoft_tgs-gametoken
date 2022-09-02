namespace Tgs.GameToken.Client;

public interface IGameTokenClient
{
	ValueTask<bool> CheckTokenValidAsync(string token, CancellationToken cancellationToken);

	ValueTask<string> CreateTokenAsync(string playerCode, CancellationToken cancellationToken);

	ValueTask<GameTokenInfo?> GetTokenInfoAsync(string token, CancellationToken cancellationToken);

	ValueTask RevokeTokenAsync(string token, CancellationToken cancellationToken);

	ValueTask<bool> CheckAuthValidAsync(string auth, CancellationToken cancellationToken);

	ValueTask<string> CreateAuthAsync(string playerCode, string info, CancellationToken cancellationToken);

	ValueTask<GameAuthInfo?> GetAuthInfoAsync(string auth, CancellationToken cancellationToken);

	ValueTask RevokeAuthAsync(string auth, CancellationToken cancellationToken);
}
