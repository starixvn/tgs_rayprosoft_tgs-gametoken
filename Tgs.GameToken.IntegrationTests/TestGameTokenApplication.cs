using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Exporter;

namespace Tgs.GameToken.IntegrationTests;

internal class TestGameTokenApplication : WebApplicationFactory<Program>
{
	private readonly Action<IWebHostBuilder>? _webHostConfigure;

	public TestGameTokenApplication(Action<IWebHostBuilder>? webHostConfigure = null)
	{
		_webHostConfigure = webHostConfigure;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		_ = builder.ConfigureServices(services =>
		{
			_ = services
				.AddFakeNatsMessageQueue()
				.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
					sp => new ConfigureNamedOptions<OtlpExporterOptions>(
						Options.DefaultName,
						exporterOptions =>
						{
							exporterOptions.TimeoutMilliseconds = 0;
						}))
				.AddSingleton<ITokenStore>(Substitute.For<ITokenStore>());
		});

		_webHostConfigure?.Invoke(builder);
	}
}
