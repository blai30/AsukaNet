using System;
using System.Data;
using Asuka.Configuration;
using Asuka.Database;
using Asuka.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Victoria;

namespace Asuka
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Initialize Serilog logger from appsettings.json configurations.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                // App settings options.
                .AddOptions()
                .Configure<TokenOptions>(Configuration.GetSection("Tokens"))
                .Configure<DiscordOptions>(Configuration.GetSection("Discord"))

                // Discord client.
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
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

                // Data access. Using factory for easy using statement and disposal.
                .AddDbContextFactory<AsukaDbContext>()

                // Http client for interfacing with Api requests.
                .AddHttpClient()

                // Mathematics.
                .AddSingleton(new Random(Guid.NewGuid().GetHashCode()))
                .AddSingleton<DataTable>()

                .AddLavaNode(config =>
                {
                    config.SelfDeaf = false;
                })

                // Background hosted services.
                .AddHostedService<LoggingService>()
                .AddHostedService<CommandHandlerService>()
                .AddHostedService<AudioService>()
                .AddHostedService<TagListenerService>()
                .AddHostedService<ReactionRoleService>()
                .AddHostedService<StartupService>()
                ;
        }
    }
}
