using Microsoft.Extensions.DependencyInjection;

namespace Tgs.GameToken.Configuration;

public class GameTokenConfiguration
{
	public IServiceCollection Services { get; }

	internal Type SourceStoreType { get; private set; } = typeof(NoopTokenStore);

	public GameTokenConfiguration(IServiceCollection services)
	{
		Services = services ?? throw new ArgumentNullException(nameof(services));
	}

	public GameTokenConfiguration Configure(Action<GameTokenOptions, IServiceProvider> configure)
	{
		_ = Services
			.AddOptions<GameTokenOptions>()
			.Configure(configure);

		return this;
	}

	public GameTokenConfiguration SetTokenStore<TTokenStore>(Func<IServiceProvider, object[]>? parametersFactory = null)
		where TTokenStore : class, ITokenStore
	{
		var sourceType = SourceStoreType;

		_ = Services.AddTransient(sp =>
		{
			var parameters = new List<object>
			{
				sp.GetRequiredService(sourceType)
			};
			if (parametersFactory is not null)
				parameters.AddRange(parametersFactory(sp));

			return ActivatorUtilities.CreateInstance<TTokenStore>(
				sp,
				parameters.ToArray());
		});

		SourceStoreType = typeof(TTokenStore);

		return this;
	}
}
