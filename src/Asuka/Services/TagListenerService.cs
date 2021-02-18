using System;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Database;
using Discord;
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
            await using var context = _factory.CreateDbContext();

            // Ensure the message is from a user or bot and not a system message.
            if (!(socketMessage is SocketUserMessage message)) return;
            // Ignore self.
            if (message.Author.Id == _client.CurrentUser.Id) return;

            var tag = await context.Tags.AsQueryable()
                .FirstOrDefaultAsync(t =>
                    string.Equals(t.Name, socketMessage.Content, StringComparison.CurrentCultureIgnoreCase));

            if (tag != null)
            {
                // Update usage count.
                tag.UsageCount++;
                context.Tags.Update(tag);

                try
                {
                    await context.SaveChangesAsync();
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
