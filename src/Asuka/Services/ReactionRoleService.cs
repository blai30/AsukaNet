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
    public class ReactionRoleService : IHostedService
    {
        private readonly ILogger<ReactionRoleService> _logger;
        private readonly IDbContextFactory<AsukaDbContext> _factory;
        private readonly DiscordSocketClient _client;

        public List<ReactionRole> ReactionRoles { get; }

        public ReactionRoleService(
            ILogger<ReactionRoleService> logger,
            IDbContextFactory<AsukaDbContext> factory,
            DiscordSocketClient client)
        {
            _logger = logger;
            _factory = factory;
            _client = client;

            ReactionRoles = new List<ReactionRole>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Populate list with data from database.
            using var context = _factory.CreateDbContext();
            var reactionRoles = context.ReactionRoles.ToList();
            ReactionRoles.AddRange(reactionRoles);

            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.MessageDeleted += OnMessageDeleted;

            _logger.LogInformation($"{GetType().Name} started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.ReactionAdded -= OnReactionAdded;
            _client.ReactionRemoved -= OnReactionRemoved;
            _client.MessageDeleted += OnMessageDeleted;

            _logger.LogInformation($"{GetType().Name} stopped");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a role to a user when a reaction was added.
        /// </summary>
        /// <param name="cachedMessage">Message from which a reaction was added</param>
        /// <param name="channel">Channel where the message is from</param>
        /// <param name="reaction">Reaction that was added</param>
        /// <returns></returns>
        private async Task OnReactionAdded(
            Cacheable<IUserMessage, ulong> cachedMessage,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            string emoteText = reaction.Emote.GetStringRepresentation();
            if (string.IsNullOrEmpty(emoteText)) return;

            // Get reaction role from list.
            var reactionRole = ReactionRoles
                .FirstOrDefault(r =>
                    r.MessageId == cachedMessage.Id &&
                    r.Emote == emoteText);

            // This reaction was not registered as a reaction role in the database.
            if (reactionRole == null) return;
            // Reaction must come from a guild user and not the bot.
            if (!(reaction.User.Value is SocketGuildUser user)) return;
            if (reaction.User.Value == _client.CurrentUser) return;
            // Check if user already has the role.
            if (user.Roles.Any(r => r.Id == reactionRole.RoleId)) return;

            // Get role by id and grant the role to the user that reacted.
            var role = user.Guild.GetRole(reactionRole.RoleId);
            try
            {
                await user.AddRoleAsync(role);
            }
            catch (HttpException e)
            {
                await channel.SendMessageAsync(
                    e +
                    "Error adding role, make sure the role " +
                    "is lower than me in the server's roles list.");
                return;
            }

            _logger.LogTrace($"Added role {role.Name} to user {user}");
        }

        /// <summary>
        /// Removes a role from a user when a reaction was removed.
        /// </summary>
        /// <param name="cachedMessage">Message from which a reaction was removed</param>
        /// <param name="channel">Channel where the message is from</param>
        /// <param name="reaction">Reaction that was removed</param>
        /// <returns></returns>
        private async Task OnReactionRemoved(
            Cacheable<IUserMessage, ulong> cachedMessage,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            string emoteText = reaction.Emote.GetStringRepresentation();
            if (string.IsNullOrEmpty(emoteText)) return;

            // Get reaction role from list.
            var reactionRole = ReactionRoles
                .FirstOrDefault(r =>
                    r.MessageId == cachedMessage.Id &&
                    r.Emote == emoteText);

            // This reaction was not registered as a reaction role in the database.
            if (reactionRole == null) return;
            // Reaction must come from a guild user.
            if (reaction.User.Value is not SocketGuildUser user) return;
            if (reaction.User.Value == _client.CurrentUser) return;
            // Check if user has the role.
            if (user.Roles.All(r => r.Id != reactionRole.RoleId)) return;

            // Get role by id and revoke the role from the user that reacted.
            var role = user.Guild.GetRole(reactionRole.RoleId);
            try
            {
                await user.RemoveRoleAsync(role);
            }
            catch (HttpException e)
            {
                await channel.SendMessageAsync(
                    e +
                    "Error removing role, make sure the role " +
                    "is lower than me in the server's roles list.");
                return;
            }

            _logger.LogTrace($"Removed role {role.Name} from user {user}");
        }

        /// <summary>
        /// When a message is deleted, all reaction roles that referenced that message
        /// will get removed from the database and cleaned out of the list.
        /// </summary>
        /// <param name="cachedMessage">Deleted message</param>
        /// <param name="channel">Channel in which the message was deleted</param>
        /// <returns></returns>
        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            // Remove all reaction roles from the list that referenced the deleted message.
            ReactionRoles.RemoveAll(reactionRole => reactionRole.MessageId == cachedMessage.Id);

            await using var context = _factory.CreateDbContext();

            // Get and remove all rows that referenced the deleted message from database.
            var rows = await context.ReactionRoles.AsQueryable()
                .Where(reactionRole => reactionRole.MessageId == cachedMessage.Id)
                .ToListAsync();

            if (!rows.Any()) return;

            context.ReactionRoles.RemoveRange(rows);
            await context.SaveChangesAsync();

            _logger.LogTrace(
                $"Deleted message ({cachedMessage.Id}), channel ({channel.Id})" +
                $" and removed {rows.Count} reaction roles");
        }
    }
}
