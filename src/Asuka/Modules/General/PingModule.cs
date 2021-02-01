using System.Threading.Tasks;
using Asuka.Commands;
using Discord.Commands;

namespace Asuka.Modules.General
{
    public class PingModule : CommandModuleBase
    {
        [Command("ping")]
        [Summary("View the latency of the bot and API.")]
        public async Task PingAsync()
        {
            var reply = await ReplyAsync("Pong! Latency: `... ms`. API: `... ms`.");
            var latency = (reply.Timestamp - Context.Message.Timestamp).Milliseconds;
            var botLatency = Context.Client.Latency;
            await reply.ModifyAsync(message => message.Content = $"Pong! Latency: `{latency} ms`. API: `{botLatency} ms`.");
        }
    }
}
