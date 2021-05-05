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
using Microsoft.Extensions.Options;
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
            // App settings options.
            services.AddOptions();
            services.Configure<TokenOptions>(Configuration.GetSection("Tokens"));
            services.Configure<DiscordOptions>(Configuration.GetSection("Discord"));
            services.Configure<LavaConfig>(Configuration.GetSection("Lavalink"));

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
                    GatewayIntents.GuildPresences |
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

            // Data access. Using factory for easy using statement and disposal.
            services.AddDbContextFactory<AsukaDbContext>();

            // Http client for interfacing with Api requests.
            services.AddHttpClient();

            // Mathematics.
            services.AddSingleton(new Random(Guid.NewGuid().GetHashCode()));
            services.AddSingleton<DataTable>();

            // Background hosted services.
            services.AddHostedService<LoggingService>();
            services.AddHostedService<CommandHandlerService>();
            services.AddHostedService<AudioService>();
            services.AddSingleton<TagListenerService>();
            services.AddHostedService(provider => provider.GetService<TagListenerService>());
            services.AddSingleton<ReactionRoleService>();
            services.AddHostedService(provider => provider.GetService<ReactionRoleService>());
            services.AddHostedService<StartupService>();
        }
    }
}
