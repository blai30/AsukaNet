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
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.ReactionAdded -= OnReactionAdded;
            _client.ReactionRemoved -= OnReactionRemoved;
            return Task.CompletedTask;
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
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
            if (!(reaction.User.Value is SocketGuildUser user)) return;
            // Check if user already has the role.
            if (user.Roles.Any(r => r.Id == reactionRole.RoleId)) return;

            // Get role by id and grant the role to the user that reacted.
            var role = user.Guild.GetRole(reactionRole.RoleId);
            await user.AddRoleAsync(role);
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
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
            // Check if user has the role.
            if (user.Roles.All(r => r.Id != reactionRole.RoleId)) return;

            // Get role by id and revoke the role from the user that reacted.
            var role = user.Guild.GetRole(reactionRole.RoleId);
            await user.RemoveRoleAsync(role);
        }
    }
}
