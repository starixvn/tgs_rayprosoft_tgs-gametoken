using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBomber.Contracts;
using NBomber.CSharp;
using Tgs.GameToken.Client;

var initStep = Step.Create("Init", context =>
{
	var builder = Host.CreateDefaultBuilder(args);

	_ = builder.ConfigureLogging(logging => logging.ClearProviders());

	_ = builder.ConfigureServices(services => _ = services.AddGameTokenClient((options, sp) => options.GameTokenUrl = new Uri("http://game-token.rpgp.cc")));

	var app = builder.Build();

	context.Data.Add("client", app.Services.GetRequiredService<IGameTokenClient>());

	return Task.FromResult(Response.Ok(statusCode: 200));
}, timeout: TimeSpan.FromSeconds(1));

var createTokenStep = Step.Create("Create Token", async context =>
{
	var client = context.Data.GetValueOrDefault("client") as IGameTokenClient;

	var token = await client!.CreateTokenAsync("test", default).ConfigureAwait(false);

	context.Data.Add("token", token);

	return Response.Ok(statusCode: 200);
}, timeout: TimeSpan.FromMilliseconds(1000));

var createTokenScenario = ScenarioBuilder.CreateScenario("Create Game Token", initStep, createTokenStep)
	.WithoutWarmUp()
	.WithLoadSimulations(Simulation.InjectPerSec(300, TimeSpan.FromSeconds(30)));

var checkTokenStep = Step.Create("Check Valid", async context =>
{
	var client = context.Data.GetValueOrDefault("client") as IGameTokenClient;
	var token = context.Data.GetValueOrDefault("token") as string;

	var isValid = await client!.CheckTokenValidAsync(token!, default).ConfigureAwait(false);

	return Response.Ok(statusCode: 200);
}, timeout: TimeSpan.FromSeconds(1));

var validTokenScenario = ScenarioBuilder.CreateScenario("Valid Game Token", initStep, createTokenStep, checkTokenStep)
	.WithoutWarmUp()
	.WithLoadSimulations(Simulation.InjectPerSec(300, TimeSpan.FromSeconds(30)));

NBomberRunner.RegisterScenarios(createTokenScenario).Run();
