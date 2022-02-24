using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tgs.GameToken;
using Tgs.GameToken.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameToken(
		this IServiceCollection services,
		Action<GameTokenConfiguration> configure)
	{
		var configuration = new GameTokenConfiguration(services);

		configure(configuration);

		_ = configuration
			.SetTokenStore<RedisTokenStore>(sp =>
			{
				var options = sp.GetRequiredService<IOptions<GameTokenOptions>>().Value;

				return new object[] { TimeSpan.FromHours(1), options.RedisDatabaseResolver(sp) };
			})
			.SetTokenStore<MemoryCachedTokenStore>(sp => new object[] { TimeSpan.FromHours(1) });
		return services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddSingleton<NoopTokenStore>()
			.AddTransient(
				sp => (ITokenStore)sp.GetRequiredService(configuration.SourceStoreType));
	}
}
