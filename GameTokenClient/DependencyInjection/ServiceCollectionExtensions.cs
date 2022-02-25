using Microsoft.Extensions.Options;
using Tgs.GameToken.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameTokenClient(
		this IServiceCollection services,
		Action<GameTokenClientOptions, IServiceProvider> configure)
	{
		return services
			.AddOptions<GameTokenClientOptions>()
			.Configure(configure)
			.Services
			.AddHttpClient<IGameTokenClient, GameTokenClient>()
			.ConfigureHttpClient((sp, httpClient) =>
			{
				var options = sp.GetRequiredService<IOptions<GameTokenClientOptions>>().Value;

				httpClient.BaseAddress = options.GameTokenUrl;
			})
			.Services;
	}
}
