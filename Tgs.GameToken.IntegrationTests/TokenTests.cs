using System;
using System.Threading;
using System.Threading.Tasks;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.GameToken.Client;
using Tgs.GameToken.MessageQueue;
using Valhalla.MessageQueue;
using Xunit;

namespace Tgs.GameToken.IntegrationTests;

public class TokenTests
{
	[Fact]
	public async Task Create_Token()
	{
		await using var web = new TestGameTokenApplication();

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CreateTokenAsync("player1", default);

		Assert.False(string.IsNullOrWhiteSpace(actual));
	}

	[Fact]
	public async Task Check_Token()
	{
		var token = "test";

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						token,
						"Player1",
						DateTime.UtcNow,
						true));

				_ = services.AddSingleton(fakeTokenStore);
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CheckTokenValidAsync(token, default);

		Assert.True(actual);
	}

	[Fact]
	public async Task Check_Invalid_Token()
	{
		var token = "test";

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new GameTokenInfo(
						token,
						"Player1",
						DateTime.UtcNow,
						false));

				_ = services.AddSingleton(fakeTokenStore);
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CheckTokenValidAsync(token, default);

		Assert.False(actual);
	}

	[Fact]
	public async Task Get_Token()
	{
		var token = "test";
		var expected = new Client.GameTokenInfo
		{
			Id = token,
			PlayerCode = "Player1",
			CreatedOn = DateTime.UtcNow,
			IsValid = true
		};

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new GameTokenInfo(
						expected.Id,
						expected.PlayerCode,
						expected.CreatedOn,
						expected.IsValid));

				_ = services.AddSingleton(fakeTokenStore);
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.GetTokenInfoAsync(token, default);

		expected.ToExpectedObject().ShouldMatch(actual);
	}

	[Fact]
	public async Task Revoke_Token()
	{
		var token = "test";
		var fakeTokenStore = Substitute.For<ITokenStore>();

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				_ = services.AddSingleton(fakeTokenStore);

				_ = services.AddSingleton(sp =>
				{
					var fakeMessageSender = Substitute.For<IMessageSender>();

					_ = fakeMessageSender.PublishAsync(
						Arg.Is("tgs.gametoken.token.revoke"),
						Arg.Any<Protos.RevokeRequest>(),
						Arg.Any<CancellationToken>())
						.Returns(callInfo =>
						{
							var data = callInfo.Arg<Protos.RevokeRequest>();
							var ct = callInfo.Arg<CancellationToken>();

							var handler = ActivatorUtilities.CreateInstance<RevokeTokenHandler>(sp);

							return handler.HandleAsync("tgs.gametoken.token.revoke", data, null, cancellationToken: ct);
						});

					return fakeMessageSender;
				});
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var cts = new CancellationTokenSource();
		cts.CancelAfter(3000);

		await sut.RevokeTokenAsync(token, cts.Token);

		_ = fakeTokenStore.Received(1)
			.DeleteTokenAsync(Arg.Is(token), Arg.Any<CancellationToken>());
	}
}
