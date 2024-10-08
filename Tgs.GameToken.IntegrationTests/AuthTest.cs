using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpectedObjects;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.GameToken.ApplicationServices;
using Tgs.GameToken.Client;
using Tgs.GameToken.MessageQueue;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;
using Xunit;

namespace Tgs.GameToken.IntegrationTests;

public class AuthTest
{
	[Fact]
	public async Task Create_Auth()
	{
		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
		{
			var fakeAuthStore = Substitute.For<IAuthStore>();

			_ = services.AddSingleton(fakeAuthStore);
		}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CreateAuthAsync("player1", "test", default);

		Assert.False(string.IsNullOrWhiteSpace(actual));
	}

	[Fact]
	public async Task Check_Auth()
	{
		var auth = "test";

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeAuthStore = Substitute.For<IAuthStore>();

				_ = fakeAuthStore.GetAuthAsync(Arg.Is(auth))
					.Returns(new GameAuthInfo(
						auth,
						"Player1",
						"test"));

				_ = services.AddSingleton(fakeAuthStore);
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CheckAuthValidAsync(auth, default);

		Assert.True(actual);
	}

	[Fact]
	public async Task Check_Invalid_Auth()
	{
		var auth = "test";

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeAuthStore = Substitute.For<IAuthStore>();

				_ = fakeAuthStore.GetAuthAsync(Arg.Is(auth))
					.Returns(new GameAuthInfo(
						auth,
						"Player1",
						"Test"));

				_ = services.AddSingleton(fakeAuthStore);
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.CheckTokenValidAsync(auth, default);

		Assert.False(actual);
	}

	[Fact]
	public async Task Get_Auth()
	{
		var auth = "test";
		var expected = new Client.GameAuthInfo
		{
			Id = auth,
			PlayerCode = "Player1",
			Info = "Test"
		};

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				var fakeAuthStore = Substitute.For<IAuthStore>();

				_ = fakeAuthStore.GetAuthAsync(Arg.Is(auth))
					.Returns(new GameAuthInfo(
						expected.Id,
						expected.PlayerCode,
						expected.Info));

				_ = services.AddSingleton(fakeAuthStore);
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.GetAuthInfoAsync(auth, default);

		expected.ToExpectedObject().ShouldMatch(actual);
	}

	[Fact]
	public async Task Revoke_Auth()
	{
		var auth = "test";
		var fakeAuthStore = Substitute.For<IAuthStore>();

		await using var web = new TestGameTokenApplication(hostBuilder =>
		_ = hostBuilder.ConfigureServices(services =>
			{
				_ = services.AddSingleton(fakeAuthStore);

				_ = services.AddSingleton(sp =>
				{
					var fakeMessageSender = Substitute.For<IMessageSender>();

					_ = fakeMessageSender.PublishAsync(
						Arg.Is("tgs.gametoken.auth.revoke"),
						Arg.Any<Protos.RevokeRequest>(),
						Arg.Any<CancellationToken>())
						.Returns(callInfo =>
						{
							var data = callInfo.Arg<Protos.RevokeRequest>();
							var ct = callInfo.Arg<CancellationToken>();

							var handler = ActivatorUtilities.CreateInstance<RevokeAuthHandler>(sp);

							return handler.HandleAsync("tgs.gametoken.auth.revoke", data, null, cancellationToken: ct);
						});

					return fakeMessageSender;
				});
			}));

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var cts = new CancellationTokenSource();
		cts.CancelAfter(3000);

		await sut.RevokeAuthAsync(auth, cts.Token);

		_ = fakeAuthStore.Received(1)
			.DeleteAuthAsync(Arg.Is(auth), Arg.Any<CancellationToken>());
	}
}
