using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Tgs.GameToken.Core.UnitTests;

public class AuthStoreTest
{
	[Fact]
	public async Task CreateToken_Will_StringSet_Value()
	{
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new AuthStore(
			NullLogger<AuthStore>.Instance,
			fakeRedisDatabase,
			TimeSpan.FromMinutes(5));

		var auth = new Auth(
			"id",
			"Player1",
			"string");

		await sut.CreateAuthAsync(auth);

		_ = fakeRedisDatabase.Received(1)
			.StringSetAsync(
				Arg.Is<RedisKey>($"Tgs:GameToken:Auth:{auth.Id}".ToLower()),
				Arg.Any<RedisValue>(),
				Arg.Is(TimeSpan.FromMinutes(5)),
				Arg.Is(When.NotExists));
	}

	[Fact]
	public async Task Get_Valid_Auth()
	{
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new AuthStore(
			NullLogger<AuthStore>.Instance,
			fakeRedisDatabase,
			TimeSpan.FromMinutes(5));

		var auth = new Auth(
			   "id",
			   "Player1",
			   "test");

		_ = fakeRedisDatabase
			.StringGetAsync(Arg.Is<RedisKey>($"Tgs:GameToken:Auth:{auth.Id}".ToLower()), Arg.Any<CommandFlags>())
			.Returns(JsonSerializer.Serialize(
				auth,
				options: new JsonSerializerOptions
				{
					WriteIndented = false,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				}));

		var actual = await sut.GetAuthAsync("id");

		Assert.NotNull(actual);
	}

	[Fact]
	public async Task Invaild_Token_Will_Return_Null()
	{
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new AuthStore(
			NullLogger<AuthStore>.Instance,
			fakeRedisDatabase,
			TimeSpan.FromMinutes(5));

		var actual = await sut.GetAuthAsync("id");

		Assert.Null(actual);
	}

	[Fact]
	public async Task If_Redis_Value_Is_Empty_Will_Return_Null()
	{
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new AuthStore(
			NullLogger<AuthStore>.Instance,
			fakeRedisDatabase,
			TimeSpan.FromMinutes(5));

		_ = fakeRedisDatabase
			.StringGetAsync(Arg.Is<RedisKey>($"Tgs:GameToken:Auth:id"))
			.Returns(string.Empty);

		var actual = await sut.GetAuthAsync("id");

		Assert.Null(actual);
	}

	[Fact]
	public async Task Delete_Token()
	{
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new AuthStore(
			NullLogger<AuthStore>.Instance,
			fakeRedisDatabase,
			TimeSpan.FromMinutes(5));

		await sut.DeleteAuthAsync("test");

		_ = await fakeRedisDatabase.Received(1)
			.KeyDeleteAsync(
			Arg.Is<RedisKey>($"Tgs:GameToken:Auth:test".ToLower()),
			Arg.Is<CommandFlags>(CommandFlags.FireAndForget));
	}
}
