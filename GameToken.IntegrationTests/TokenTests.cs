using System;
using System.Threading.Tasks;
using ExpectedObjects;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.GameToken;
using Tgs.GameToken.Clients;

namespace GameToken.IntegrationTests;

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
		{
			hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						token,
						"Player1",
						DateTime.UtcNow,
						true));

				services.AddSingleton(fakeTokenStore);
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
			hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						token,
						"Player1",
						DateTime.UtcNow,
						false));

				services.AddSingleton(fakeTokenStore);
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
		var expected = new Tgs.GameToken.Clients.GameTokenInfo
		{
			Id = token,
			PlayerCode = "Player1",
			CreatedDate = DateTime.UtcNow,
			IsValid = true
		};

		await using var web = new TestGameTokenApplication(hostBuilder =>
		{
			hostBuilder.ConfigureServices(services =>
			{
				var fakeTokenStore = Substitute.For<ITokenStore>();

				_ = fakeTokenStore.GetTokenAsync(Arg.Is(token))
					.Returns(new Tgs.GameToken.GameTokenInfo(
						expected.Id,
						expected.PlayerCode,
						expected.CreatedDate,
						expected.IsValid));

				services.AddSingleton(fakeTokenStore);
			});
		});

		var httpClient = web.CreateClient();

		var sut = new GameTokenClient(httpClient);

		var actual = await sut.GetTokenInfoAsync(token, default);

		expected.ToExpectedObject().ShouldMatch(actual);
	}
}
