namespace Tgs.GameToken;

public sealed class InitStartup(ITokenStore tokenStore) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await tokenStore.InitializationAsync(
			cancellationToken)
			.ConfigureAwait(false);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
