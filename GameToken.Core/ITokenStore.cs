namespace Tgs.GameToken;

public interface ITokenStore
{
	ValueTask CreateTokenAsync(Token token);

	ValueTask<GameTokenInfo?> GetTokenAsync(string id);
}
