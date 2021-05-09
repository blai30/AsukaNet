using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Models.Api.Asuka;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Flurl;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asuka.Services
{
    public class TagListenerService : IHostedService
    {
        private const string Uri = "https://localhost:5001/api/tags";

        private readonly DiscordSocketClient _client;
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<TagListenerService> _logger;

        public TagListenerService(
            DiscordSocketClient client,
            IHttpClientFactory factory,
            ILogger<TagListenerService> logger)
        {
            _client = client;
            _factory = factory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;

            _logger.LogInformation($"{GetType().Name} started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived -= OnMessageReceived;

            _logger.LogInformation($"{GetType().Name} stopped");
            await Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            // Ensure the message is from a user or bot, is not a system message, and is from a guild channel.
            if (socketMessage is not SocketUserMessage { Channel: SocketGuildChannel guildChannel } message) return;
            // Ignore self.
            if (message.Author.Id == _client.CurrentUser.Id) return;

            string query = Uri
                .SetQueryParam("name", message.Content)
                .SetQueryParam("guildId", guildChannel.Guild.Id.ToString());

            using var client = _factory.CreateClient();
            var response = await client.GetFromJsonAsync<IEnumerable<Tag>>(query);

            var tag = response?.FirstOrDefault();

            if (tag is null) return;

            // Respond to tag with content and optional reaction.
            using IDisposable typingState = message.Channel.EnterTypingState();
            await message.ReplyAsync(tag.Content, allowedMentions: AllowedMentions.None);

            if (string.IsNullOrEmpty(tag.Reaction)) return;
            // Parse emote or emoji.
            IEmote reaction = Emote.TryParse(tag.Reaction, out var emote)
                ? (IEmote) emote
                : new Emoji(tag.Reaction);

            // Add reaction will error when adding a reaction that cannot be parsed.
            try
            {
                await message.AddReactionAsync(reaction);
            }
            catch (HttpException e)
            {
                _logger.LogTrace(e.Message);
                _logger.LogTrace($"Could not add reaction for tag: {tag.Name}");
            }
        }
    }
}
