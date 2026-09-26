using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LeagueOps.Yahoo;

namespace LeagueOps.Functions;

public class YahooAuth
{
    private readonly YahooOptions _options;
    private readonly ILogger<YahooAuth> _logger;

    public YahooAuth(IOptions<YahooOptions> options, ILogger<YahooAuth> logger)
    {
        _options = options?.Value ?? new YahooOptions();
        _logger = logger;
    }

    [Function("YahooAuth")]
    public async Task<HttpResponseData> Run([
        HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "yahoo/auth")]
        HttpRequestData req)
    {
        _logger.LogInformation("YahooAuth triggered. Method={Method} Path={Path} Query={Query}", req.Method, req.Url.AbsolutePath, req.Url.Query);

        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.RedirectUri))
        {
            _logger.LogWarning("YahooOptions.ClientId or RedirectUri is not configured. ClientIdSet={HasClientId} RedirectUriSet={HasRedirect}",
                !string.IsNullOrEmpty(_options.ClientId), !string.IsNullOrEmpty(_options.RedirectUri));
        }

        var clientId = Uri.EscapeDataString(_options.ClientId ?? string.Empty);
        var redirectUri = Uri.EscapeDataString(_options.RedirectUri ?? string.Empty);

        // var authorizationUrl =
        //     $"https://api.login.yahoo.com/oauth2/request_auth" +
        //     $"?client_id={clientId}" +
        //     $"&redirect_uri={redirectUri}" +
        //     $"&response_type=code";
        var authorizationUrl =
            "https://api.login.yahoo.com/oauth2/request_auth" +
            $"?client_id={_options.ClientId}" +
            $"&redirect_uri={_options.RedirectUri}" +
            "&response_type=code" +
            "&prompt=consent";        

        _logger.LogDebug("Generated Yahoo authorization URL: {AuthorizationUrl}", authorizationUrl);

        var response = req.CreateResponse(HttpStatusCode.Redirect);
        response.Headers.Add("Location", authorizationUrl);

        _logger.LogInformation("Redirecting to Yahoo authorization endpoint.");

        return response;
    }
}
