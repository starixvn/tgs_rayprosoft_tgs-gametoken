namespace Tgs.GameToken;

public interface IAuthStore
{
	public ValueTask CreateAuthAsync(Auth auth);

	public ValueTask<GameAuthInfo?> GetAuthAsync(string id);

	public ValueTask DeleteAuthAsync(string id);
}
