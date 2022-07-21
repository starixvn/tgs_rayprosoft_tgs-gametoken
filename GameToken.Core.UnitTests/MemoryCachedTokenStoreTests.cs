using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.GameToken;

namespace GameToken.Core.UnitTests;

[TestClass]
public class MemoryCachedTokenStoreTests
{
	[TestMethod]
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
				 true);
		fakeMemoryCache.Set($"Tgs:GameToken:id", token);

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNotNull(actual);
		Assert.AreEqual(token.PlayerCode, actual.PlayerCode);

		_ = fakeSourceTokenStore.Received(1)
			.GetTokenAsync(Arg.Is("id"));
	}

	[TestMethod]
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
				 true);
		fakeSourceTokenStore.GetTokenAsync(Arg.Is(token.Id))
			.Returns(token);

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNotNull(actual);
		Assert.AreEqual(token.PlayerCode, actual.PlayerCode);

		_ = fakeSourceTokenStore.Received(2)
			.GetTokenAsync(Arg.Is("id"));
	}

	[TestMethod]
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
				 JsonSerializer.Serialize(new
				 {
					 uid = 1,
					 nickname = "Player1",
					 currency = "IDN",
					 table = "B",
					 language = "id",
					 clientIP = "127.0.0.1"
				 }),
				 DateTime.UtcNow);

		await sut.CreateTokenAsync(token);

		_ = fakeSourceTokenStore.Received(1)
			.CreateTokenAsync(Arg.Is<Token>(t => t.Id == token.Id
				&& t.PlayerCode == token.PlayerCode
				&& t.CreatedDate == token.CreatedDate));
	}

	[TestMethod]
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
		entry.SetValue(null);
		entry.Dispose();

		Assert.IsTrue(fakeMemoryCache.TryGetValue(key, out _));

		await sut.DeleteTokenAsync("test");

		_ = fakeSourceTokenStore.Received(1)
			.DeleteTokenAsync(Arg.Is("test"));
		Assert.IsFalse(fakeMemoryCache.TryGetValue(key, out _));
	}
}
