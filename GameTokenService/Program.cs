using Microsoft.AspNetCore.Mvc;
using Tgs.GameToken;

var builder = WebApplication.CreateBuilder(args);

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
var app = builder.Build();

app.MapControllers();
app.MapSwagger();
app.UseReDoc();
app.UseSwaggerUI();

app.Run();
