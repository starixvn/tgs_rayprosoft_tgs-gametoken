namespace Tgs.GameToken.Client;

public class GameTokenApiException : Exception
{
	public GameTokenApiException(string? message) : base(message)
	{
	}
}
