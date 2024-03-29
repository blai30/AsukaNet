﻿using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Asuka.Services;

public class LoggingService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(
        DiscordSocketClient client,
        InteractionService interactionService,
        ILogger<LoggingService> logger)
    {
        _client = client;
        _interactionService = interactionService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _client.Ready += OnReadyAsync<StartupService>;
        _client.Log += OnLogAsync<StartupService>;
        _interactionService.Log += OnLogAsync<InteractionHandlerService>;

        _logger.LogInformation($"{GetType().Name} started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Ready -= OnReadyAsync<StartupService>;
        _client.Log -= OnLogAsync<StartupService>;
        _interactionService.Log -= OnLogAsync<InteractionHandlerService>;

        _logger.LogInformation($"{GetType().Name} stopped");
        await Task.CompletedTask;
    }

    private async Task OnReadyAsync<T>() where T : IHostedService
    {
        var logger = Log.ForContext<T>();
        logger.Information($"Client logged in as {_client.CurrentUser}");
        logger.Information($"Listening in {_client.Guilds.Count.ToString()} guilds");

        await Task.CompletedTask;
    }

    private async Task OnLogAsync<T>(LogMessage log) where T : IHostedService
    {
        var logger = Log.ForContext<T>();
        string message = $"{log.Exception?.ToString() ?? log.Message}";

        switch (log.Severity)
        {
            case LogSeverity.Critical:
                logger.Fatal(message);
                break;
            case LogSeverity.Error:
                logger.Error(message);
                break;
            case LogSeverity.Warning:
                logger.Warning(message);
                break;
            case LogSeverity.Info:
                logger.Information(message);
                break;
            case LogSeverity.Debug:
                logger.Debug(message);
                break;
            case LogSeverity.Verbose:
                logger.Verbose(message);
                break;
            default:
                logger.Information(message);
                break;
        }

        await Task.CompletedTask;
    }
}
