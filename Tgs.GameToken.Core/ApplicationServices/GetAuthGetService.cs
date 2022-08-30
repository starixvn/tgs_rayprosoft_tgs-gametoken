using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class GetAuthGetService : IGetService<GetAuthQuery, GameAuthInfo?>
{
	private readonly IAuthStore _tokenStore;

	public GetAuthGetService(IAuthStore tokenStore)
	{
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
	}

	public ValueTask<GameAuthInfo?> GetAsync(GetAuthQuery query, CancellationToken cancellationToken = default)
		=> _tokenStore.GetAuthAsync(query.Id);
}
