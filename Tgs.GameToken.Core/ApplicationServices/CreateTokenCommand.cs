namespace Tgs.GameToken.ApplicationServices;

public record CreateTokenCommand(
	string PlayerCode,
	string Info);
