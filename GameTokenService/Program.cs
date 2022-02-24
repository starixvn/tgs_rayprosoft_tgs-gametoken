using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GameToken;
using Tgs.GameToken.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
			fluentOptions.UseMessagePack();
		});
	});

builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers()
	.AddMvcOptions(options =>
	{
		options.AddWebApiToolkitFilters();
	});

builder.Services
	.AddMemoryCache()
	.AddSingleton(sp =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		return new RedisConnectionManager(config.GetConnectionString("Redis"));
	})
	.AddSingleton(sp =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		return new MongoConnectionManager(config.GetConnectionString("Mongo"));
	})
	.AddGameToken(configuration =>
	{
		configuration
			.Configure((options, sp) =>
			{
				options.RedisDatabaseResolver =
					serviceProvider => serviceProvider.GetRequiredService<RedisConnectionManager>().Database;
			})
			.SetMongoDbTokenStore(sp => sp.GetRequiredService<MongoConnectionManager>().Database);
	});

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig =>
	{
		natsConfig
			.ConfigQueueOptions((options, sp) =>
			{
				var config = sp.GetRequiredService<IConfiguration>();

				options.Url = config.GetConnectionString("Nats");
			})
			.AddHandler<RevokeTokenHandler>("tgs.game-token.revoke");
	})
	.AddNatsGlobPatternExchange("*");

builder.Services
	 .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
		sp => new ConfigureNamedOptions<OtlpExporterOptions>(
			Microsoft.Extensions.Options.Options.DefaultName,
			exporterOptions =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var nodeIp = config.GetValue<string>("NODE_IP");

				if (!string.IsNullOrWhiteSpace(nodeIp))
					exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

				config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
			}))
	.AddOpenTelemetryTracing(traceBuilder => traceBuilder
		.SetResourceBuilder(ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.AddProfileViewer()
	.Configure<AspNetCoreInstrumentationOptions>(options =>
	{
		options.Filter = httpContext =>
		{
			return !httpContext.Request.Path.HasValue
				|| !httpContext.Request.Path.StartsWithSegments("/swagger")
				&& !httpContext.Request.Path.StartsWithSegments("/healthz")
				&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
				&& !new[] { "html", "ico", "png", "jpg" }.Any(
					ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
		};
	});

builder.Services
	.AddSwaggerGen(options =>
	{
		options.SwaggerDoc(
			"v1",
			new Microsoft.OpenApi.Models.OpenApiInfo
			{
				Title = "GameToken Service",
				Version = "v1"
			});

		options.ApplyWebApiToolkitSupport();
		options.ApplyDisplayNameSchemaId();
	});

builder.Services
	.AddHealthChecks();

var app = builder.Build();

app.MapControllers();
app.MapProfileViewer();
app.MapSwagger();
app.UseReDoc();
app.UseSwaggerUI();
app.MapHealthChecks("/healthz");

app.Run();
