using Tgs.GameToken;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers();

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
	});
var app = builder.Build();

app.MapControllers();
app.MapSwagger();
app.UseReDoc();
app.UseSwaggerUI();

app.Run();
