using System;
using System.Data;
using Asuka.Configuration;
using Asuka.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Victoria;

Console.WriteLine(DateTime.UtcNow.ToString("R"));
Console.WriteLine(Environment.ProcessId);

var host = Host.CreateDefaultBuilder(args);
host.UseSerilog();

host.ConfigureServices((builder, services) =>
{
    // Add services to the container.

    // Initialize Serilog logger from appsettings.json configurations.
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();

    // App settings options.
    services.AddOptions();
    services.Configure<ApiOptions>(builder.Configuration.GetSection("AsukaApi"));
    services.Configure<TokenOptions>(builder.Configuration.GetSection("Tokens"));
    services.Configure<DiscordOptions>(builder.Configuration.GetSection("Discord"));
    services.Configure<LavaConfig>(builder.Configuration.GetSection("Lavalink"));

    services.AddHttpClient();
    services.AddSingleton(new Random(Guid.NewGuid().GetHashCode()));
    services.AddSingleton<DataTable>();

    // Discord client.
    var client = new DiscordSocketClient(new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Verbose,
        MessageCacheSize = 1000,
        GatewayIntents =
            GatewayIntents.Guilds |
            GatewayIntents.GuildMembers |
            GatewayIntents.GuildMessages |
            GatewayIntents.GuildMessageReactions |
            GatewayIntents.GuildVoiceStates
    });

    services.AddSingleton(client);
    services.AddSingleton(new InteractionService(client, new InteractionServiceConfig
    {
        LogLevel = LogSeverity.Verbose,
        DefaultRunMode = RunMode.Async,
        UseCompiledLambda = true
    }));

    // Audio server using Lavalink.
    services.AddSingleton(provider => provider.GetRequiredService<IOptions<LavaConfig>>().Value);
    services.AddSingleton<LavaNode>();

    // Background hosted services.
    services.AddHostedService<LoggingService>();
    services.AddHostedService<InteractionHandlerService>();
    services.AddHostedService<AudioService>();
    services.AddHostedService<RoleAssignerService>();
    services.AddHostedService<StartupService>();
});

var app = host.Build();
await app.RunAsync();
