using System.Reflection;
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
			.SetTokenStore<MemoryCachedTokenStore>(sp => new object[] { TimeSpan.FromMinutes(5) });
		return services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddSingleton<NoopTokenStore>()
			.AddTransient(
				sp => (ITokenStore)sp.GetRequiredService(configuration.SourceStoreType))
			.AddTransient<IAuthStore>(sp =>
			{
				var options = sp.GetRequiredService<IOptions<GameTokenOptions>>().Value;
				return ActivatorUtilities.CreateInstance<AuthStore>(sp, options.RedisDatabaseResolver(sp), TimeSpan.FromMinutes(5));
			});
	}
}
