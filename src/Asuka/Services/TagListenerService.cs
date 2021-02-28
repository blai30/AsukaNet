using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Database;
using Asuka.Database.Models;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asuka.Services
{
    public class TagListenerService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly IDbContextFactory<AsukaDbContext> _factory;
        private readonly ILogger<TagListenerService> _logger;

        public TagListenerService(
            DiscordSocketClient client,
            IDbContextFactory<AsukaDbContext> factory,
            ILogger<TagListenerService> logger)
        {
            _client = client;
            _factory = factory;
            _logger = logger;
        }

        public Dictionary<int, Tag> Tags { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Fetch all from database and store in dictionary for fast access.
            await using var context = _factory.CreateDbContext();
            Tags = await context.Tags.AsNoTracking()
                .ToDictionaryAsync(tag => tag.Id, cancellationToken);

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
            // Ensure the message is from a user or bot and not a system message.
            if (!(socketMessage is SocketUserMessage message)) return;
            // Ensure message is from a guild channel.
            if (!(message.Channel is SocketGuildChannel guildChannel)) return;
            // Ignore self.
            if (message.Author.Id == _client.CurrentUser.Id) return;

            // Get tag from dictionary by guild id and name.
            var tag = Tags.Values
                .FirstOrDefault(t =>
                    t.GuildId == guildChannel.Guild.Id &&
                    string.Equals(t.Name, message.Content, StringComparison.CurrentCultureIgnoreCase));

            if (tag == null) return;

            await using var context = _factory.CreateDbContext();
            // Update usage count for both the tag object from dictionary and the entry in the database.
            var entity = await context.Tags
                .AsQueryable()
                .FirstOrDefaultAsync(t => t.Id == tag.Id);

            entity.UsageCount++;
            tag.UsageCount++;

            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                _logger.LogError($"Error updating usage count for tag {tag.Name}, id ({tag.Id})");
                throw;
            }

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
