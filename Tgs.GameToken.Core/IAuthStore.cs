namespace Tgs.GameToken;

public interface IAuthStore
{
	public ValueTask CreateAuthAsync(Auth auth, CancellationToken cancellationToken = default);

	public ValueTask<GameAuthInfo?> GetAuthAsync(string id, CancellationToken cancellationToken = default);

	public ValueTask DeleteAuthAsync(string id, CancellationToken cancellationToken = default);
}
