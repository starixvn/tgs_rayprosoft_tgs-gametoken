using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NSubstitute;
using StackExchange.Redis;
using Tgs.GameToken;

namespace GameToken.Core.UnitTests;

[TestClass]
public class RedisTokenStoreTests
{
	[TestMethod]
	public async Task CreateToken_Will_StringSet_Value()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromHours(1),
			fakeRedisDatabase,
			NullLogger<RedisTokenStore>.Instance);

		var token = new Token("id", "Player1", DateTime.UtcNow);

		await sut.CreateTokenAsync(token);

		_ = fakeRedisDatabase.Received(1)
			.StringSetAsync(
				Arg.Is<RedisKey>($"Tgs:GameToken:{token.Id}"),
				Arg.Any<RedisValue>(),
				Arg.Is(TimeSpan.FromHours(1)),
				Arg.Is(When.NotExists),
				Arg.Any<CommandFlags>());
	}

	[TestMethod]
	public async Task Get_Valid_Token()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromHours(1),
			fakeRedisDatabase,
			NullLogger<RedisTokenStore>.Instance);

		var token = new Token("id", "Player1", DateTime.UtcNow);

		fakeRedisDatabase
			.StringGetAsync(Arg.Is<RedisKey>($"Tgs:GameToken:{token.Id}"), Arg.Any<CommandFlags>())
			.Returns(JsonSerializer.Serialize(
				token,
				new JsonSerializerOptions
				{
					WriteIndented = false,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				}));

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.IsValid);
	}

	[TestMethod]
	public async Task Invaild_Token_Will_Return_Null()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromHours(1),
			fakeRedisDatabase,
			NullLogger<RedisTokenStore>.Instance);

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task If_Redis_Value_Is_Empty_Will_Return_Null()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromHours(1),
			fakeRedisDatabase,
			NullLogger<RedisTokenStore>.Instance);
		fakeRedisDatabase
			.StringGetAsync(Arg.Is<RedisKey>("Tgs:GameToken:id"), Arg.Any<CommandFlags>())
			.Returns(string.Empty);

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task Delete_Token()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromHours(1),
			fakeRedisDatabase,
			NullLogger<RedisTokenStore>.Instance);

		await sut.DeleteTokenAsync("test");

		_ = fakeSourceTokenStore.Received(1)
			.DeleteTokenAsync(Arg.Is("test"));

		_ = fakeRedisDatabase.Received(1)
			.KeyDeleteAsync(Arg.Is<RedisKey>("Tgs:GameToken:test"));
	}
}
