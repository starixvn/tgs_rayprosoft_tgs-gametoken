using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class DeleteAuthCommandService : ICommandService<DeleteAuthCommand>
{
	private readonly IAuthStore _tokenStore;

	public DeleteAuthCommandService(IAuthStore tokenStore)
	{
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
	}

	public ValueTask ExecuteAsync(DeleteAuthCommand query, CancellationToken cancellationToken = default)
		=> _tokenStore.DeleteAuthAsync(query.Id);
}
