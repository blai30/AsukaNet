using System;
using System.Net.Http;
using Asuka.Configuration;
using Asuka.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Asuka
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Initialize Serilog logger from appsettings.json configurations.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                // App settings options.
                .AddOptions()
                .Configure<TokenOptions>(Configuration.GetSection("Tokens"))
                .Configure<DiscordOptions>(Configuration.GetSection("Discord"))
                .Configure<DatabaseOptions>(Configuration.GetSection("Database"))

                // Reusable random number generator with random GUID seed.
                .AddSingleton(new Random(Guid.NewGuid().GetHashCode()))

                // Http client for interfacing with Api requests.
                .AddSingleton<HttpClient>()

                // Discord client.
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000,
                    GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMembers |
                        GatewayIntents.GuildMessages |
                        GatewayIntents.GuildMessageReactions |
                        GatewayIntents.GuildPresences |
                        GatewayIntents.GuildVoiceStates
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false,
                    IgnoreExtraArgs = true
                }))

                // Background hosted services.
                .AddHostedService<StartupService>()
                .AddHostedService<LoggingService>()
                .AddHostedService<CommandHandlerService>()
                ;
        }
    }
}
