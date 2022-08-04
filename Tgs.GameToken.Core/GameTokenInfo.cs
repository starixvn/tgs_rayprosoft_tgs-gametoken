namespace Tgs.GameToken;

public record GameTokenInfo(
	string Id,
	string PlayerCode,
	string Info,
	DateTime CreatedDate,
	bool IsValid);
