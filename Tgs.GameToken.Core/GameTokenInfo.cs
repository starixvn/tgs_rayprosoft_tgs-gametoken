namespace Tgs.GameToken;

public record GameTokenInfo(
	string Id,
	string PlayerCode,
	DateTime CreatedOn,
	bool IsValid);
