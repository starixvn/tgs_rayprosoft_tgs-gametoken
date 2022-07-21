namespace Tgs.GameToken.Clients;

public record GameTokenInfo
{
	public string Id { get; init; } = default!;

	public string PlayerCode { get; init; } = default!;

	public string Info { get; init; } = default!;

	public DateTime CreatedDate { get; init; }

	public bool IsValid { get; init; }
}
