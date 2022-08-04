using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.GameToken.MessageQueue;
using Tgs.MessageQueue;
using Tgs.GameToken.Client;

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

		var info = JsonSerializer.Serialize(new
		{
			uid = 1,
			nickname = "Player1",
			currency = "IDN",
			table = "B",
			language = "id",
			clientIP = "127.0.0.1"
		});

		var actual = await sut.CreateTokenAsync("player1", info, default);

		Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
	}

	[TestMethod]
	public async Task Check_Token()
	{
		var token = "test";

		await using var web = new TestGameTokenApplication(hostBuilder =>
		{
			_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						token,
						"Player1",
						JsonSerializer.Serialize(new
						{
							uid = 1,
							nickname = "Player1",
							currency = "IDN",
							table = "B",
							language = "id",
							clientIP = "127.0.0.1"
						}),
						DateTime.UtcNow,
						true));

				_ = services.AddSingleton(fakeTokenStore);
			});
		});

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
		{
			_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						token,
						"Player1",
						JsonSerializer.Serialize(new
						{
							uid = 1,
							nickname = "Player1",
							currency = "IDN",
							table = "B",
							language = "id",
							clientIP = "127.0.0.1"
						}),
						DateTime.UtcNow,
						false));

				_ = services.AddSingleton(fakeTokenStore);
			});
		});

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CheckTokenValidAsync(token, default);

		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Get_Token()
	{
		var token = "test";
		var expected = new Tgs.GameToken.Client.GameTokenInfo
		{
			Id = token,
			PlayerCode = "Player1",
			Info = JsonSerializer.Serialize(new
			{
				uid = 1,
				nickname = "Player1",
				currency = "IDN",
				table = "B",
				language = "id",
				clientIP = "127.0.0.1"
			}),
			CreatedDate = DateTime.UtcNow,
			IsValid = true
		};

		await using var web = new TestGameTokenApplication(hostBuilder =>
		{
			_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						expected.Id,
						expected.PlayerCode,
						expected.Info,
						expected.CreatedDate,
						expected.IsValid));

				_ = services.AddSingleton(fakeTokenStore);
			});
		});

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
		{
			_ = hostBuilder.ConfigureServices(services =>
			{
				_ = services.AddSingleton(fakeTokenStore);

				_ = services.AddSingleton(sp =>
				{
					var fakeMessageSender = Substitute.For<IMessageSender>();

					_ = fakeMessageSender.SendAsync(
						Arg.Is("tgs.game-token.revoke"),
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
			});
		});

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var cts = new CancellationTokenSource();
		cts.CancelAfter(3000);

		await sut.RevokeTokenAsync(token, cts.Token);

		_ = fakeTokenStore.Received(1)
			.DeleteTokenAsync(Arg.Is(token));
	}
}
