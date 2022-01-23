using System;
using System.Data;
using Asuka.Configuration;
using Asuka.Services;
using Discord;
using Discord.Commands;
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

    // Discord client.
    services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Verbose,
        MessageCacheSize = 1000,
        GatewayIntents =
            GatewayIntents.DirectMessages |
            GatewayIntents.DirectMessageReactions |
            GatewayIntents.DirectMessageTyping |
            GatewayIntents.Guilds |
            GatewayIntents.GuildMembers |
            GatewayIntents.GuildMessages |
            GatewayIntents.GuildMessageReactions |
            GatewayIntents.GuildVoiceStates
    }));
    services.AddSingleton(new CommandService(new CommandServiceConfig
    {
        LogLevel = LogSeverity.Verbose,
        DefaultRunMode = RunMode.Async,
        CaseSensitiveCommands = false,
        IgnoreExtraArgs = true
    }));

    // Audio server using Lavalink.
    services.AddSingleton(provider => provider.GetRequiredService<IOptions<LavaConfig>>().Value);
    services.AddSingleton<LavaNode>();

    // Http client for interfacing with Api requests.
    services.AddHttpClient();

    // Mathematics.
    services.AddSingleton(new Random(Guid.NewGuid().GetHashCode()));
    services.AddSingleton<DataTable>();

    // Background hosted builder.Services.
    services.AddHostedService<LoggingService>();
    services.AddHostedService<CommandHandlerService>();
    services.AddHostedService<AudioService>();
    services.AddHostedService<TagListenerService>();
    services.AddHostedService<ReactionRoleService>();
    services.AddHostedService<StartupService>();
});

var app = host.Build();

await app.RunAsync();

// Typical ASP.NET host builder pattern but for console app without the web.
// TODO: Official support planned for .NET 6.0.
// var host = Host
//     .CreateDefaultBuilder(args)
//     .UseSerilog()
//     .UseStartup<Startup>()
//     .Build();
