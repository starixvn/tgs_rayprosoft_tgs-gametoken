namespace Tgs.GameToken.Client;

public record GameTokenInfo
{
	public string Id { get; init; } = default!;

	public string PlayerCode { get; init; } = default!;

	public DateTime CreatedOn { get; init; }

	public bool IsValid { get; init; }
}
