using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
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
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack();
		});
	});

builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers()
	.AddMvcOptions(options => options.AddWebApiToolkitFilters());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
	foreach (var xmlFileName in xmlFileNames)
		c.IncludeXmlComments(xmlFileName);
});

builder.Services
	.AddMemoryCache()
	.AddSingleton(sp =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		return new RedisConnectionManager(config.GetConnectionString("Redis")!);
	})
	.AddSingleton(sp =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		return new MongoConnectionManager(config.GetConnectionString("Mongo")!);
	})
	.AddGameToken(configuration => configuration
		.Configure((options, sp) =>
		options.RedisDatabaseResolver =
		serviceProvider => serviceProvider.GetRequiredService<RedisConnectionManager>().Database)
		.SetMongoDbTokenStore(sp => sp.GetRequiredService<MongoConnectionManager>().Database));

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig =>
	natsConfig
		.ConfigQueueOptions((options, sp) =>
		{
			var config = sp.GetRequiredService<IConfiguration>();
			options.Url = config.GetConnectionString("Nats")!;
		})
		.AddHandler<RevokeTokenHandler>("tgs.gametoken.token.revoke")
		.AddHandler<RevokeAuthHandler>("tgs.gametoken.auth.revoke"))
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddOpenTelemetry()
	.WithTracing(builder => builder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddGrpcClientInstrumentation()
		.AddGrpcCoreInstrumentation()
		.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
	.SetResourceBuilder(ResourceBuilder.CreateDefault()
	.AddEnvironmentVariableDetector())
	.AddRuntimeInstrumentation()
	.AddAspNetCoreInstrumentation()
	.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !httpContext.Request.Path.StartsWithSegments("/hangfire")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();
app.MapSwagger();
app.UseReDoc();
app.UseSwaggerUI();
app.MapHealthChecks("/healthz");

app.Run();
