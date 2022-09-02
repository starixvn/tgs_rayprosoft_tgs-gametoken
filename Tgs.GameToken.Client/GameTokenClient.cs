using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Tgs.GameToken.Client;

internal class GameTokenClient : IGameTokenClient
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly HttpClient _httpClient;

	public GameTokenClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public async ValueTask<string> CreateTokenAsync(string playerCode, CancellationToken cancellationToken)
	{
		var requestContent = new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PostAsync(
				$"/token/{playerCode}",
				requestContent,
				cancellationToken)
			.ConfigureAwait(false);

		switch (response.StatusCode)
		{
			case HttpStatusCode.Created:
				{
					var contentJson = await response.Content
						.ReadAsStringAsync(cancellationToken)
						.ConfigureAwait(false);

					return JsonSerializer.Deserialize<string>(contentJson, _SerializerOptions)!;
				}
			default:
				throw new GameTokenApiException("Create token fail.");
		}
	}

	public async ValueTask<GameTokenInfo?> GetTokenInfoAsync(string token, CancellationToken cancellationToken)
	{
		var response = await _httpClient
			.GetAsync($"/token/{token}", cancellationToken)
			.ConfigureAwait(false);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				{
					var contentJson = await response.Content
						.ReadAsStringAsync(cancellationToken)
						.ConfigureAwait(false);

					return JsonSerializer.Deserialize<GameTokenInfo>(contentJson, _SerializerOptions)!;
				}
			case HttpStatusCode.NotFound:
				return null;

			default:
				throw new GameTokenApiException("Unknown error");
		}
	}

	public async ValueTask<bool> CheckTokenValidAsync(string token, CancellationToken cancellationToken)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Head, $"/token/{token}")
		{
			Content = new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json)
		};

		var response = await _httpClient
			.SendAsync(requestMessage, cancellationToken)
			.ConfigureAwait(false);

		return response.StatusCode switch
		{
			HttpStatusCode.OK => true,
			HttpStatusCode.NotFound => false,
			HttpStatusCode.NotAcceptable => false,
			_ => throw new GameTokenApiException("Unknown error")
		};
	}

	public async ValueTask RevokeTokenAsync(string token, CancellationToken cancellationToken)
	{
		var response = await _httpClient
			.DeleteAsync($"/token/{token}", cancellationToken)
			.ConfigureAwait(false);

		if (response.StatusCode is not HttpStatusCode.Accepted)
			throw new GameTokenApiException("Unknown error");
	}

	public async ValueTask<bool> CheckAuthValidAsync(string auth, CancellationToken cancellationToken)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Head, $"/auth/{auth}")
		{
			Content = new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json)
		};

		var response = await _httpClient
			.SendAsync(requestMessage, cancellationToken)
			.ConfigureAwait(false);

		return response.StatusCode switch
		{
			HttpStatusCode.OK => true,
			HttpStatusCode.NotFound => false,
			HttpStatusCode.NotAcceptable => false,
			_ => throw new GameTokenApiException("Unknown error")
		};
	}

	public async ValueTask<string> CreateAuthAsync(string playerCode, string info, CancellationToken cancellationToken)
	{
		var request = new
		{
			Info = info
		};
		var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PostAsync(
				$"/auth/{playerCode}",
				requestContent,
				cancellationToken)
			.ConfigureAwait(false);

		switch (response.StatusCode)
		{
			case HttpStatusCode.Created:
				{
					var contentJson = await response.Content
						.ReadAsStringAsync(cancellationToken)
						.ConfigureAwait(false);

					return JsonSerializer.Deserialize<string>(contentJson, _SerializerOptions)!;
				}
			default:
				throw new GameTokenApiException("Create token fail.");
		}
	}

	public async ValueTask<GameAuthInfo?> GetAuthInfoAsync(string auth, CancellationToken cancellationToken)
	{
		var response = await _httpClient
			.GetAsync($"/auth/{auth}", cancellationToken)
			.ConfigureAwait(false);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				{
					var contentJson = await response.Content
						.ReadAsStringAsync(cancellationToken)
						.ConfigureAwait(false);

					return JsonSerializer.Deserialize<GameAuthInfo>(contentJson, _SerializerOptions)!;
				}
			case HttpStatusCode.NotFound:
				return null;

			default:
				throw new GameTokenApiException("Unknown error");
		}
	}

	public async ValueTask RevokeAuthAsync(string auth, CancellationToken cancellationToken)
	{
		var response = await _httpClient
			.DeleteAsync($"/auth/{auth}", cancellationToken)
			.ConfigureAwait(false);

		if (response.StatusCode is not HttpStatusCode.Accepted)
			throw new GameTokenApiException("Unknown error");
	}
}
