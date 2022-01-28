using System;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General;

public class PingModule : InteractionModule
{
    public PingModule(
        IOptions<DiscordOptions> config,
        ILogger<PingModule> logger) :
        base(config, logger)
    {
    }

    [SlashCommand(
        "ping",
        "View the latency of the bot and API.")]
    public async Task PingAsync()
    {
        const string text = "Pong! Latency: `{0} ms`. API: `{1} ms`.";

        int latency = (Context.Interaction.CreatedAt - DateTimeOffset.Now).Milliseconds;
        int botLatency = Context.Client.Latency;

        await RespondAsync(string.Format(text, latency.ToString(), botLatency.ToString()));
    }
}
