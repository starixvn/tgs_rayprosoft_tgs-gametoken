using System;
using System.Threading;
using System.Threading.Tasks;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.GameToken.Client;
using Tgs.GameToken.MessageQueue;
using Tgs.MessageQueue;

namespace Tgs.GameToken.IntegrationTests;

[TestClass]
public class TokenTests
{
	[TestMethod]
	public async Task Create_Token()
	{
		await using var web = new TestGameTokenApplication();

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CreateTokenAsync("player1", default);

		Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
	}

	[TestMethod]
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

		Assert.IsTrue(actual);
	}

	[TestMethod]
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

		Assert.IsFalse(actual);
	}

	[TestMethod]
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

	[TestMethod]
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
						Arg.Any<ReadOnlyMemory<byte>>(),
						Arg.Any<CancellationToken>())
						.Returns(callInfo =>
						{
							var data = callInfo.Arg<ReadOnlyMemory<byte>>();
							var ct = callInfo.Arg<CancellationToken>();

							var handler = ActivatorUtilities.CreateInstance<RevokeTokenHandler>(sp);

							return handler.HandleAsync(data, ct);
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
			.DeleteTokenAsync(Arg.Is(token));
	}
}
