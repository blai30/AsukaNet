using System.Threading.Tasks;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Commands.Modules.General
{
    [Group("ping")]
    [Remarks("General")]
    [Summary("View the latency of the bot and API.")]
    public class PingModule : CommandModuleBase
    {
        public PingModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command]
        [Remarks("ping")]
        public async Task PingAsync()
        {
            var reply = await ReplyAsync("Pong! Latency: `... ms`. API: `... ms`.");
            var latency = (reply.Timestamp - Context.Message.Timestamp).Milliseconds;
            var botLatency = Context.Client.Latency;
            await reply.ModifyAsync(message =>
                message.Content = $"Pong! Latency: `{latency} ms`. API: `{botLatency} ms`.");
        }
    }
}
