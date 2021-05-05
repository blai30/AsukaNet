﻿using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General
{
    [Group("ping")]
    [Remarks("General")]
    [Summary("View the latency of the bot and API.")]
    public class PingModule : CommandModuleBase
    {
        public PingModule(
            IOptions<DiscordOptions> config,
            ILogger<PingModule> logger) :
            base(config, logger)
        {
        }

        [Command]
        [Remarks("ping")]
        public async Task PingAsync()
        {
            const string text = "Pong! Latency: `{0} ms`. API: `{1} ms`.";

            var reply = await ReplyAsync(string.Format(text, "... ms", "... ms"));
            int latency = (reply.Timestamp - Context.Message.Timestamp).Milliseconds;
            int botLatency = Context.Client.Latency;

            await reply.ModifyAsync(message =>
                message.Content = string.Format(text, latency.ToString(), botLatency.ToString()));
        }
    }
}
