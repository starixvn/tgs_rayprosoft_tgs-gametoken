using Microsoft.Extensions.DependencyInjection;

namespace Tgs.GameToken.Configuration;

public class GameTokenConfiguration(IServiceCollection services)
{
	public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

	internal Type SourceStoreType { get; private set; } = typeof(NoopTokenStore);

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
				[.. parameters]);
		});

		SourceStoreType = typeof(TTokenStore);

		return this;
	}
}
