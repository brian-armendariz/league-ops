using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LeagueOps.Yahoo;

public sealed class YahooFantasyClient(ILogger<YahooFantasyClient> logger, HttpClient httpClient)
{
    public async Task<string> GetLeagueAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://fantasysports.yahooapis.com/fantasy/v2/users;use_login=1/games;game_keys=nfl/leagues?format=json");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        logger.LogInformation(
            "AFTER SEND: StatusCode={StatusCode}, IsSuccessStatusCode={IsSuccess}",
            response.StatusCode,
            response.IsSuccessStatusCode);

        var content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        logger.LogInformation(
            "AFTER READ: StatusCode={StatusCode}, ContentLength={Length}",
            response.StatusCode,
            content.Length);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "ENTERED FAILURE BLOCK. StatusCode={StatusCode}, Body={Body}",
                response.StatusCode,
                content);

            throw new HttpRequestException(
                $"Yahoo Fantasy API request failed. " +
                $"Status={(int)response.StatusCode}. Response={content}");
        }

        logger.LogInformation("PASSED FAILURE CHECK");

        return content;
    }
}