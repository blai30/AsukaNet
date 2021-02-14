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

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            throw new NotImplementedException();
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            throw new NotImplementedException();
        }
    }
}
