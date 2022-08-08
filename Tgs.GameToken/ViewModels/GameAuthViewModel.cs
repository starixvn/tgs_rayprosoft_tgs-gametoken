using System.ComponentModel;

namespace Tgs.GameToken.ViewModels;

[DisplayName("GameAuth")]
public record GameAuthViewModel
{
	public string Id { get; init; } = default!;

	public string PlayerCode { get; init; } = default!;

	public string Info { get; init; } = default!;
}
