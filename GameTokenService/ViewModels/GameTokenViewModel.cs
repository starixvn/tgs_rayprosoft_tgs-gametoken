using System.ComponentModel;

namespace Tgs.GameToken.ViewModels;

[DisplayName("GameToken")]
public class GameTokenViewModel
{
	public string Id { get; init; } = default!;

	public string PlayerCode { get; init; } = default!;

	public DateTime CreatedDate { get; init; }

	public bool IsValid { get; init; }
}
