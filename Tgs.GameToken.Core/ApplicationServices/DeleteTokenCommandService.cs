using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class DeleteTokenCommandService : ICommandService<DeleteTokenCommand>
{
	private readonly ITokenStore _tokenStore;

	public DeleteTokenCommandService(ITokenStore tokenStore)
	{
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
	}

	public ValueTask ExecuteAsync(DeleteTokenCommand command, CancellationToken cancellationToken = default)
		=> _tokenStore.DeleteTokenAsync(command.Id);
}
