using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Populate list with data from database.
            await using var context = _factory.CreateDbContext();
            var reactionRoles = context.ReactionRoles.ToList();
            ReactionRoles.AddRange(reactionRoles);

            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.ReactionsCleared += OnReactionsCleared;
            _client.ReactionsRemovedForEmote += OnReactionsRemovedForEmote;
            _client.RoleDeleted += OnRoleDeleted;
            _client.MessageDeleted += OnMessageDeleted;

            _logger.LogInformation($"{GetType().Name} started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.ReactionAdded -= OnReactionAdded;
            _client.ReactionRemoved -= OnReactionRemoved;
            _client.ReactionsCleared -= OnReactionsCleared;
            _client.ReactionsRemovedForEmote -= OnReactionsRemovedForEmote;
            _client.RoleDeleted -= OnRoleDeleted;
            _client.MessageDeleted -= OnMessageDeleted;

            _logger.LogInformation($"{GetType().Name} stopped");
            await Task.CompletedTask;
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
            // This event is not related to reaction roles.
            if (ReactionRoles.All(r => r.MessageId != cachedMessage.Id))
            {
                return;
            }

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
            if (reaction.User.Value is not SocketGuildUser user) return;
            if (reaction.User.Value.Id == _client.CurrentUser.Id) return;
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
            // This event is not related to reaction roles.
            if (ReactionRoles.All(r => r.MessageId != cachedMessage.Id))
            {
                return;
            }

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
            if (reaction.User.Value is not SocketGuildUser user) return;
            if (reaction.User.Value.Id == _client.CurrentUser.Id) return;
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
        /// Remove all reaction roles from the database that referenced the message when all reactions from the message get cleared.
        /// </summary>
        /// <param name="cachedMessage">Message whose reactions got cleared</param>
        /// <param name="channel">Channel in which the reactions of the message was cleared</param>
        /// <returns></returns>
        private async Task OnReactionsCleared(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            await ClearReactionRoles(cachedMessage.Id, channel);
        }

        /// <summary>
        /// Remove all reactions to a specific emote from the database that referenced the message when its reactions was cleared.
        /// </summary>
        /// <param name="cachedMessage">Message whose reaction got cleared</param>
        /// <param name="channel">Channel in which the reaction of the message was cleared</param>
        /// <param name="reaction">Reaction that was cleared</param>
        /// <returns></returns>
        private async Task OnReactionsRemovedForEmote(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, IEmote reaction)
        {
            await ClearReactionRoles(cachedMessage.Id, channel, reaction);
        }

        /// <summary>
        /// Remove reaction roles from the list and database when a guild deletes a role.
        /// Clears reactions from all messages that referenced the deleted role.
        /// </summary>
        /// <param name="role">Deleted role</param>
        /// <returns></returns>
        private async Task OnRoleDeleted(SocketRole role)
        {
            // Remove reaction roles from list.
            var reactionRoles = ReactionRoles.Where(reactionRole => reactionRole.RoleId == role.Id).ToList();
            foreach (var reactionRole in reactionRoles)
            {
                // Parse emote or emoji.
                IEmote reaction = Emote.TryParse(reactionRole.Emote, out var emote)
                    ? (IEmote) emote
                    : new Emoji(reactionRole.Emote);

                var channel = _client.GetChannel(reactionRole.ChannelId) as ISocketMessageChannel;
                if (channel == null) continue;

                // Clear reactions from message, this will trigger the OnReactionsRemovedForEmote event
                // which will handle the removal of reaction roles from the list and database.
                var message = await channel.GetMessageAsync(reactionRole.MessageId);
                await message.RemoveAllReactionsForEmoteAsync(reaction);

                // Prevent hitting rate limit.
                Thread.Sleep(100);
            }
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
            await ClearReactionRoles(cachedMessage.Id, channel);
        }

        /// <summary>
        /// Remove all reaction roles from the database for a specific reaction or all reactions from a message.
        /// </summary>
        /// <param name="messageId">Id of the message to clear reactions from</param>
        /// <param name="channel">Channel in which the message is referenced</param>
        /// <param name="reaction">Specific reaction to clear from message. If none is specified, clear all reactions from message.</param>
        /// <returns></returns>
        private async Task ClearReactionRoles(ulong messageId, ISocketMessageChannel channel, IEmote reaction = null)
        {
            // This event is not related to reaction roles.
            if (ReactionRoles.All(r => r.MessageId != messageId))
            {
                return;
            }

            // Condition to remove all reaction roles from a message if no reaction was specified,
            // otherwise only remove all reaction roles for that specific reaction.
            Expression<Func<ReactionRole, bool>> expression = reaction == null
                ? reactionRole => reactionRole.MessageId == messageId
                : reactionRole => reactionRole.MessageId == messageId &&
                                  reactionRole.Emote == reaction.GetStringRepresentation();

            Predicate<ReactionRole> predicate = expression.Compile().Invoke;

            // Remove all reaction roles from the list that referenced the message.
            ReactionRoles.RemoveAll(predicate);

            await using var context = _factory.CreateDbContext();

            // Get and remove all rows that referenced the message from database.
            var rows = await context.ReactionRoles.AsQueryable()
                .Where(expression)
                .ToListAsync();

            if (!rows.Any()) return;

            context.ReactionRoles.RemoveRange(rows);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogTrace(
                    $"Removed {rows.Count} reaction roles from message ({messageId}), channel ({channel.Id})");
            }
            catch
            {
                _logger.LogError($"Error removing reaction roles from message ({messageId}), channel ({channel.Id})");
                throw;
            }
        }
    }
}
