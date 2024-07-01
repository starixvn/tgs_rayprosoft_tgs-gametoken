using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameToken.ApplicationServices;
using Tgs.GameToken.Protos;
using Tgs.GameToken.ViewModels;
using Tgs.ServiceUtility;
using Tgs.WebApi.Toolkits;
using Valhalla.MessageQueue;

namespace Tgs.GameToken.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	[HttpPost("{playerCode}", Name = "CreateAuth")]
	[SuccessStatusCode(201, "Auth created")]
	[Produces(MediaTypeNames.Application.Json)]
	public ValueTask<string> CreateAuthAsync(
		[FromServices] IGetService<CreateAuthQuery, string> createTokenService,
		string playerCode,
		[FromBody] CreateAuthViewModel model)
		=> createTokenService.GetAsync(new CreateAuthQuery(
			playerCode,
			model.Info));

	[HttpHead("{id}", Name = "CheckAuthValid")]
	[SuccessStatusCode(200, "Auth valid")]
	[FailStatusCode(404, "Auth not found")]
	public async Task CheckAuthAsync(
		[FromServices] IGetService<GetAuthQuery, GameAuthInfo?> getTokenService,
		string id)
	{
		var query = new GetAuthQuery(id);

		var tokenInfo = await getTokenService
			.GetAsync(query, HttpContext.RequestAborted)
			.ConfigureAwait(false);

		if (tokenInfo is null)
		{
			await this.FailAsync(404, "Auth invalid").ConfigureAwait(false);
			return;
		}
	}

	[HttpGet("{id}", Name = "GetAuthInfo")]
	[SuccessStatusCode(200, "Auth found")]
	[FailStatusCode(404, "Auth not found")]
	public async Task<GameAuthViewModel?> GetAuthAsync(
		[FromServices] IGetService<GetAuthQuery, GameAuthInfo?> getTokenService,
		string id)
	{
		var query = new GetAuthQuery(id);

		var tokenInfo = await getTokenService
			.GetAsync(query, HttpContext.RequestAborted)
			.ConfigureAwait(false);

		if (tokenInfo is null)
		{
			this.Fail<GameTokenViewModel?>(404, null);

			return default;
		}

		return new GameAuthViewModel
		{
			Id = tokenInfo.Id,
			PlayerCode = tokenInfo.PlayerCode,
			Info = tokenInfo.Info
		};
	}

	[HttpDelete("{id}", Name = "RevokeAuth")]
	[SuccessStatusCode(202, "Revoke accept")]
	public ValueTask RevokeAuthAsync(
		[FromServices] IMessageSender messageSender,
		string id)
	{
		var request = new RevokeRequest
		{
			Id = id
		};

		return messageSender.PublishAsync(
			subject: "tgs.gametoken.auth.revoke",
			data: request,
			cancellationToken: HttpContext.RequestAborted);
	}
}
