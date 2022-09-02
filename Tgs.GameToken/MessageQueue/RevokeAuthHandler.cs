using Tgs.GameToken.ApplicationServices;
using Tgs.GameToken.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameToken.MessageQueue;

public class RevokeAuthHandler : IMessageHandler
{
	private readonly ICommandService<DeleteAuthCommand> _deleteTokenService;

	public RevokeAuthHandler(ICommandService<DeleteAuthCommand> deleteTokenService)
	{
		_deleteTokenService = deleteTokenService ?? throw new ArgumentNullException(nameof(deleteTokenService));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = RevokeRequest.Parser.ParseFrom(data.Span);

		var command = new DeleteAuthCommand(request.Id);

		return _deleteTokenService
				.ExecuteAsync(
					command: command,
					cancellationToken: cancellationToken);
	}
}
