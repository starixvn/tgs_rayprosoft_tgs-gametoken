using Tgs.GameToken.ApplicationServices;
using Tgs.GameToken.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameToken.MessageQueue;

internal class RevokeTokenHandler(
	ILogger<RevokeAuthHandler> logger,
	ICommandService<DeleteTokenCommand> deleteTokenService) : IMessageHandler<RevokeRequest>
{
	public ValueTask HandleAsync(string subject, RevokeRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			new
			{
				data.Id
			});

		return deleteTokenService.ExecuteAsync(
			command: new(data.Id),
			cancellationToken: cancellationToken);
	}
}
