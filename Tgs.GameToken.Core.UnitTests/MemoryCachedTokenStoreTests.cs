using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Tgs.GameToken.Core.UnitTests;

public class MemoryCachedTokenStoreTests
{
	[Fact]
	public async Task Get_Token_Hit_Cache()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeMemoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

		var sut = new MemoryCachedTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromMinutes(5),
			fakeMemoryCache);

		var token = new GameTokenInfo(
			   "id",
				 "Player1",
				 DateTime.UtcNow,
				 true);
		_ = fakeMemoryCache.Set($"Tgs:GameToken:id", token);

		var actual = await sut.GetTokenAsync("id");

		Assert.NotNull(actual);
		Assert.Equal(token.PlayerCode, actual!.PlayerCode);

		_ = await fakeSourceTokenStore.Received(1)
			.GetTokenAsync(Arg.Is("id"));
	}

	[Fact]
	public async Task Get_Token_Miss_Cache()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeMemoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

		var sut = new MemoryCachedTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromMinutes(5),
			fakeMemoryCache);

		var token = new GameTokenInfo(
			   "id",
				 "Player1",
				 DateTime.UtcNow,
				 true);
		_ = fakeSourceTokenStore.GetTokenAsync(Arg.Is(token.Id))
			.Returns(token);

		var actual = await sut.GetTokenAsync("id");

		Assert.NotNull(actual);
		Assert.Equal(token.PlayerCode, actual!.PlayerCode);

		_ = await fakeSourceTokenStore.Received(2)
			.GetTokenAsync(Arg.Is("id"));
	}

	[Fact]
	public async Task Create_Token_Will_Pass_To_Source()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeMemoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

		var sut = new MemoryCachedTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromMinutes(5),
			fakeMemoryCache);

		var token = new Token(
			   "id",
				 "Player1",
				 DateTime.UtcNow);

		await sut.CreateTokenAsync(token);

		await fakeSourceTokenStore.Received(1)
				.CreateTokenAsync(Arg.Is<Token>(t => t.Id == token.Id
					&& t.PlayerCode == token.PlayerCode
					&& t.CreatedOn == token.CreatedOn));
	}

	[Fact]
	public async Task Delete_Token()
	{
		var fakeSourceTokenStore = Substitute.For<ITokenStore>();
		var fakeMemoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

		var sut = new MemoryCachedTokenStore(
			fakeSourceTokenStore,
			TimeSpan.FromMinutes(5),
			fakeMemoryCache);

		var key = "Tgs:GameToken:test";
		var entry = fakeMemoryCache.CreateEntry(key);
		_ = entry.SetValue(value: null);
		entry.Dispose();

		Assert.True(fakeMemoryCache.TryGetValue(key, out _));

		await sut.DeleteTokenAsync("test");

		await fakeSourceTokenStore
			.Received(1)
			.DeleteTokenAsync(
			Arg.Is("test"),
			Arg.Any<CancellationToken>());

		Assert.False(fakeMemoryCache.TryGetValue(key, out _));
	}
}
