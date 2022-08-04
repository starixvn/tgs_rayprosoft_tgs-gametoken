using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class GetTokenQueryService : IGetService<GetTokenQuery, GameTokenInfo?>
{
	private readonly ITokenStore _tokenStore;

	public GetTokenQueryService(ITokenStore tokenStore)
	{
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
	}

	public async ValueTask<GameTokenInfo?> GetAsync(GetTokenQuery query, CancellationToken cancellationToken = default)
	{
		var token = await _tokenStore
			.GetTokenAsync(query.Id)
			.ConfigureAwait(false);

		return token;
	}
}
