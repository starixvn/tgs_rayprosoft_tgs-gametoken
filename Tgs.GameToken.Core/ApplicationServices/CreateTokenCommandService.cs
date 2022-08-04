using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class CreateTokenCommandService : IGetService<CreateTokenCommand, string>
{
	private readonly ITokenStore _tokenStore;

	public CreateTokenCommandService(ITokenStore tokenStore)
	{
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
	}

	public async ValueTask<string> GetAsync(CreateTokenCommand command, CancellationToken cancellationToken = default)
	{
		var newId = Guid.NewGuid().ToString("n");

		var token = new Token(
			newId,
			command.PlayerCode,
			command.Info,
			DateTime.UtcNow);

		await _tokenStore.CreateTokenAsync(token).ConfigureAwait(false);

		return newId;
	}
}
