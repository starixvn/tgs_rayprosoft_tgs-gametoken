using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class CreateAuthGetService : IGetService<CreateAuthQuery, string>
{
	private readonly IAuthStore _tokenStore;

	public CreateAuthGetService(IAuthStore tokenStore)
	{
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
	}

	public async ValueTask<string> GetAsync(CreateAuthQuery query, CancellationToken cancellationToken = default)
	{
		var newId = Guid.NewGuid().ToString("n");

		var auth = new Auth(
			newId,
			query.PlayerCode,
			query.Info);

		await _tokenStore.CreateAuthAsync(auth).ConfigureAwait(false);

		return newId;
	}
}
