using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using LeagueOps.Yahoo.Models;

namespace LeagueOps.Yahoo;

public sealed class YahooOAuthClient
{
    private const string TokenEndpoint =
        "https://api.login.yahoo.com/oauth2/get_token";

    private readonly HttpClient _httpClient;
    private readonly YahooOptions _options;
    private readonly ILogger<YahooOAuthClient> _logger;

    public YahooOAuthClient(
        ILogger<YahooOAuthClient> logger,
        HttpClient httpClient,
        IOptions<YahooOptions> options)
    {
        _logger = logger;
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<YahooTokenResponse> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        // POST authorization code + redirect URI
        // to Yahoo's token endpoint

        var credentials = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(
                $"{_options.ClientId}:{_options.ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", _options.RedirectUri }
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);
            
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Yahoo token exchange failed. " +
                $"Status={(int)response.StatusCode}. Response={content}");
        }

        var tokenResponse =
            await response.Content.ReadFromJsonAsync<YahooTokenResponse>(
                cancellationToken);

        _logger.LogInformation(
            "Yahoo token received. HasAccessToken={HasAccessToken}, TokenType={TokenType}",
            !string.IsNullOrWhiteSpace(tokenResponse?.AccessToken),
            tokenResponse?.TokenType);                

        return tokenResponse
            ?? throw new InvalidOperationException(
                "Yahoo returned an empty token response.");
    }   
}