using System.Net.Mime;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameToken.ApplicationServices;
using Tgs.GameToken.Protos;
using Tgs.GameToken.ViewModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using Tgs.WebApi.Toolkits;

namespace Tgs.GameToken.Controllers;

[Route("[controller]")]
[ApiController]
public class TokenController : ControllerBase
{
	[HttpPost("{playerCode}", Name = "CreateToken")]
	[SuccessStatusCode(201, "Token created")]
	[Produces(MediaTypeNames.Application.Json)]
	public ValueTask<string> CreateTokenAsync(
		[FromServices] IGetService<CreateTokenCommand, string> createTokenService,
		string playerCode,
		[FromBody] CreateTokenViewModel model)
		=> createTokenService.GetAsync(new CreateTokenCommand(
			playerCode,
			model.Info));

	[HttpHead("{id}", Name = "CheckTokenValid")]
	[SuccessStatusCode(200, "Token valid")]
	[FailStatusCode(404, "Token not found")]
	[FailStatusCode(406, "Token invalid")]
	public async Task CheckTokenAsync(
		[FromServices] IGetService<GetTokenQuery, GameTokenInfo?> getTokenService,
		string id)
	{
		var query = new GetTokenQuery(id);

		var tokenInfo = await getTokenService
			.GetAsync(query, HttpContext.RequestAborted)
			.ConfigureAwait(false);

		if (tokenInfo is null)
		{
			await this.FailAsync(404, "Token invalid").ConfigureAwait(false);
			return;
		}

		if (!tokenInfo.IsValid)
		{
			await this.FailAsync(406, "Token invalid").ConfigureAwait(false);
			return;
		}
	}

	[HttpGet("{id}", Name = "GetTokenInfo")]
	[SuccessStatusCode(200, "Token found")]
	[FailStatusCode(404, "Token not found")]
	public async Task<GameTokenViewModel?> GetTokenAsync(
		[FromServices] IGetService<GetTokenQuery, GameTokenInfo?> getTokenService,
		string id)
	{
		var query = new GetTokenQuery(id);

		var tokenInfo = await getTokenService
			.GetAsync(query, HttpContext.RequestAborted)
			.ConfigureAwait(false);

		if (tokenInfo is null)
		{
			this.Fail<GameTokenViewModel?>(404, null);

			return default;
		}

		return new GameTokenViewModel
		{
			Id = tokenInfo.Id,
			PlayerCode = tokenInfo.PlayerCode,
			Info = tokenInfo.Info,
			CreatedDate = tokenInfo.CreatedDate,
			IsValid = tokenInfo.IsValid
		};
	}

	[HttpDelete("{id}", Name = "RevokeToken")]
	[SuccessStatusCode(202, "Revoke accept")]
	public ValueTask RevokeTokenAsync(
		[FromServices] IMessageSender messageSender,
		string id)
	{
		var request = new RevokeRequest
		{
			Id = id
		};

		return messageSender.SendAsync(
			"tgs.game-token.revoke",
			request.ToByteArray(),
			HttpContext.RequestAborted);
	}
}
