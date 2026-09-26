using Azure.Monitor.OpenTelemetry.Exporter;
using LeagueOps.Yahoo;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
// {
//     builder.Services.AddOpenTelemetry()
//         .UseFunctionsWorkerDefaults()
//         .UseAzureMonitorExporter();
// }

builder.Services.Configure<YahooOptions>(
    builder.Configuration.GetSection("Yahoo"));
builder.Services.AddHttpClient<YahooOAuthClient>();
builder.Services.AddHttpClient<YahooFantasyClient>();
// builder.Services.AddHttpClient<DiscordWebhookClient>();

// builder.Services.AddScoped<IWeeklyLeagueService, YahooWeeklyLeagueService>();

builder.Build().Run();
