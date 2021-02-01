using System;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asuka
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public Startup(string[] args)
        {
            CancellationTokenSource = new CancellationTokenSource();
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Development.json", true, true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            await provider.GetRequiredService<LoggingService>().StartAsync(CancellationTokenSource.Token);
            await provider.GetRequiredService<StartupService>().StartAsync(CancellationTokenSource.Token);
            await provider.GetRequiredService<CommandHandlerService>().StartAsync(CancellationTokenSource.Token);
            // await provider.GetRequiredService<ReactionRoleService>().StartAsync(CancellationTokenSource.Token);

            await Task.Delay(-1);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // App settings options.
            services
                .AddOptions()
                .Configure<TokenOptions>(Configuration.GetSection("Tokens"))
                .Configure<DiscordOptions>(Configuration.GetSection("Discord"))
                .Configure<DatabaseOptions>(Configuration.GetSection("Database"));

            // Discord client.
            services
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
                    CaseSensitiveCommands = false
                }))
                .AddSingleton<StartupService>()
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<Random>()
                .AddSingleton(CancellationTokenSource)
                .AddSingleton(Configuration);
        }
    }
}
