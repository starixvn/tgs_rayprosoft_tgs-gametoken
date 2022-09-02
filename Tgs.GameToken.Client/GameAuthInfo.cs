namespace Tgs.GameToken.Client;
public record GameAuthInfo
{
	public string Id { get; init; } = default!;

	public string PlayerCode { get; init; } = default!;

	public string Info { get; init; } = default!;
}
