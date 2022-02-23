namespace Tgs.GameToken.Clients;

public class GameTokenApiException : Exception
{
	public GameTokenApiException(string? message) : base(message)
	{
	}
}
