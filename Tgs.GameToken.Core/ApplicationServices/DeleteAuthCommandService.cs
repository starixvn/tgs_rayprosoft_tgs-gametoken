using Tgs.ServiceUtility;

namespace Tgs.GameToken.ApplicationServices;

internal class DeleteAuthCommandService(IAuthStore tokenStore) : ICommandService<DeleteAuthCommand>
{
	public ValueTask ExecuteAsync(DeleteAuthCommand query, CancellationToken cancellationToken = default)
		=> tokenStore.DeleteAuthAsync(query.Id, cancellationToken);
}
