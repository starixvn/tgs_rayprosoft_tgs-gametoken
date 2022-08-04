using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Tgs.GameToken.Core.UnitTests;

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
		_ = fakeMemoryCache.Set($"Tgs:GameToken:id", token);

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNotNull(actual);
		Assert.AreEqual(token.PlayerCode, actual.PlayerCode);

		_ = await fakeSourceTokenStore.Received(1)
			.GetTokenAsync(Arg.Is("id")).ConfigureAwait(false);
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
		_ = fakeSourceTokenStore.GetTokenAsync(Arg.Is(token.Id))
			.Returns(token);

		var actual = await sut.GetTokenAsync("id");

		Assert.IsNotNull(actual);
		Assert.AreEqual(token.PlayerCode, actual.PlayerCode);

		_ = await fakeSourceTokenStore.Received(2)
			.GetTokenAsync(Arg.Is("id")).ConfigureAwait(false);
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

		await fakeSourceTokenStore.Received(1)
				.CreateTokenAsync(Arg.Is<Token>(t => t.Id == token.Id
					&& t.PlayerCode == token.PlayerCode
					&& t.CreatedDate == token.CreatedDate)).ConfigureAwait(false);
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
		_ = entry.SetValue(null);
		entry.Dispose();

		Assert.IsTrue(fakeMemoryCache.TryGetValue(key, out _));

		await sut.DeleteTokenAsync("test");

		await fakeSourceTokenStore.Received(1)
			.DeleteTokenAsync(Arg.Is("test")).ConfigureAwait(false);

		Assert.IsFalse(fakeMemoryCache.TryGetValue(key, out _));
	}
}
