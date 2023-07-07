using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.GameToken.ApplicationServices;

namespace Tgs.GameToken.Core.UnitTests;

[TestClass]
public class TokenTests
{
	[TestMethod]
	public async Task CreateToken()
	{
		var fakeTokenStore = Substitute.For<ITokenStore>();

		var sut = new CreateTokenCommandService(fakeTokenStore);

		var command = new CreateTokenCommand(
			  PlayerCode: "Player1");

		var before = DateTime.UtcNow;
		_ = await sut.GetAsync(command);
		var after = DateTime.UtcNow;

		await fakeTokenStore.Received(1)
				.CreateTokenAsync(Arg.Is<Token>(token => token.PlayerCode == command.PlayerCode
					&& token.CreatedOn >= before
					&& token.CreatedOn <= after)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task GetToken()
	{
		var fakeTokenStore = Substitute.For<ITokenStore>();

		var sut = new GetTokenQueryService(fakeTokenStore);

		var query = new GetTokenQuery("tokenId");

		_ = fakeTokenStore.GetTokenAsync(Arg.Is(query.Id))
			.Returns(new GameTokenInfo(
				 query.Id,
				 "Player1",
				 DateTime.UtcNow,
				true));

		var actual = await sut.GetAsync(query);

		Assert.AreEqual("Player1", actual!.PlayerCode);
	}

	[TestMethod]
	public async Task GetToken_NotFound_Should_Return_Null()
	{
		var fakeTokenStore = Substitute.For<ITokenStore>();

		var sut = new GetTokenQueryService(fakeTokenStore);

		var query = new GetTokenQuery("tokenId");

		var actual = await sut.GetAsync(query);

		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task DeleteToken()
	{
		var fakeTokenStore = Substitute.For<ITokenStore>();

		var sut = new DeleteTokenCommandService(fakeTokenStore);

		var command = new DeleteTokenCommand("test");

		await sut.ExecuteAsync(command);

		await fakeTokenStore.Received(1)
			.DeleteTokenAsync(Arg.Is(command.Id)).ConfigureAwait(false);
	}
}
