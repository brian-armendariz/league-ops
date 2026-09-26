using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using LeagueOps.Yahoo;

namespace LeagueOps.Functions;

public class YahooCallback
{
    private readonly YahooOAuthClient _yahooOAuthClient;
    private readonly YahooFantasyClient _yahooFantasyClient;
    private readonly ILogger<YahooCallback> _logger;

    public YahooCallback(
        ILogger<YahooCallback> logger,
        YahooOAuthClient yahooOAuthClient,
        YahooFantasyClient yahooFantasyClient)
    {
        _logger = logger;
        _yahooOAuthClient = yahooOAuthClient;
        _yahooFantasyClient = yahooFantasyClient;
    }

    [Function("YahooCallback")]
    public async Task<HttpResponseData> Run([
        HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "yahoo/callback")]
        HttpRequestData req)
    {
        _logger.LogInformation("Yahoo OAuth callback triggered.");

        var query = QueryHelpers.ParseQuery(req.Url.Query);
        query.TryGetValue("code", out StringValues codeVals);
        query.TryGetValue("error", out StringValues errorVals);

        var code = codeVals.ToString();
        var error = errorVals.ToString();

        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("Yahoo callback returned error={Error}", error);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync($"Error during Yahoo OAuth callback: {error}");
            return response;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Yahoo callback missing authorization code.");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Missing authorization code in Yahoo OAuth callback.");
            return response;
        }

        var tokenResponse = await _yahooOAuthClient.ExchangeCodeAsync(code);
        _logger.LogInformation("Exchanged authorization code for Yahoo access token.");

        try
        {
            var leagueResponse = await _yahooFantasyClient.GetLeagueAsync(tokenResponse.AccessToken);
            _logger.LogInformation("Retrieved Yahoo Fantasy league information.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add(
                "Content-Type",
                "application/json; charset=utf-8");

            await response.WriteStringAsync(
                leagueResponse);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving Yahoo Fantasy league information.");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving Yahoo Fantasy league information.");
            return response;
        }

    }
}