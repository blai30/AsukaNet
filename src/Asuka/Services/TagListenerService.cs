using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Database;
using Asuka.Database.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asuka.Services
{
    public class TagListenerService : IHostedService
    {
        private readonly ILogger<TagListenerService> _logger;
        private readonly IDbContextFactory<AsukaDbContext> _factory;
        private readonly DiscordSocketClient _client;

        public List<Tag> Tags { get; }

        public TagListenerService(
            ILogger<TagListenerService> logger,
            IDbContextFactory<AsukaDbContext> factory,
            DiscordSocketClient client)
        {
            _logger = logger;
            _factory = factory;
            _client = client;

            Tags = new List<Tag>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Populate list with data from database.
            await using var context = _factory.CreateDbContext();
            var tags = context.Tags.ToList();
            Tags.AddRange(tags);

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

        public void UpdateTag(int tagId, string tagContent)
        {
            Tags[Tags.FindIndex(tag => tag.Id == tagId)].Content = tagContent;
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            // Ensure the message is from a user or bot and not a system message.
            if (!(socketMessage is SocketUserMessage message)) return;

            // Ignore self.
            if (message.Author.Id == _client.CurrentUser.Id) return;

            var tag = Tags.FirstOrDefault(t => string.Equals(t.Name, socketMessage.Content, StringComparison.CurrentCultureIgnoreCase));
            if (tag != null)
            {
                // Update usage count.
                await using var context = _factory.CreateDbContext();
                tag.UsageCount++;
                context.Tags.Update(tag);

                try
                {
                    await context.SaveChangesAsync();
                    Tags[Tags.FindIndex(t => t.Id == tag.Id)].UsageCount++;
                }
                catch
                {
                    _logger.LogError($"Error updating usage count for tag {tag.Name}, id ({tag.Id})");
                    throw;
                }

                await message.ReplyAsync(tag.Content, allowedMentions: AllowedMentions.None);
            }
        }
    }
}
