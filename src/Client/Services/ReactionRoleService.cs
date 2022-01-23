using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Models.Api.Asuka;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Flurl;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Services;

public class ReactionRoleService : IHostedService
{
    private readonly IOptions<ApiOptions> _api;
    private readonly DiscordSocketClient _client;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<ReactionRoleService> _logger;

    public ReactionRoleService(
        IOptions<ApiOptions> api,
        DiscordSocketClient client,
        IHttpClientFactory factory,
        ILogger<ReactionRoleService> logger)
    {
        _api = api;
        _client = client;
        _factory = factory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
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
    ///     Adds a role to a user when a reaction was added.
    /// </summary>
    /// <param name="message">Message from which a reaction was added</param>
    /// <param name="channel">Channel where the message is from</param>
    /// <param name="reaction">Reaction that was added</param>
    /// <returns></returns>
    private async Task OnReactionAdded(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        // Reaction must come from a guild user and not the bot.
        if (reaction.User.Value is not SocketGuildUser user) return;
        if (reaction.User.Value.Id == _client.CurrentUser.Id) return;
        // Ensure message is from a guild channel.
        if (channel.Value is not SocketGuildChannel guildChannel) return;

        string emoteText = reaction.Emote.GetStringRepresentation();
        if (string.IsNullOrEmpty(emoteText)) return;

        string query = _api.Value.ReactionRolesUri
            .SetQueryParam("guildId", guildChannel.Guild.Id.ToString())
            .SetQueryParam("messageId", message.Id.ToString());

        using var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<IEnumerable<ReactionRole>>(query);

        // Get reaction role.
        var reactionRole = response?
            .FirstOrDefault(r => r.Reaction == emoteText);

        // This reaction was not registered as a reaction role in the database.
        if (reactionRole is null) return;
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
            await channel.Value.SendMessageAsync(
                $"{e.Message}\nError adding role, make sure the role is lower than me in the server's roles list.");
            return;
        }

        _logger.LogTrace($"Added role {role.Name} to user {user}");
    }

    /// <summary>
    ///     Removes a role from a user when a reaction was removed.
    /// </summary>
    /// <param name="cachedMessage">Message from which a reaction was removed</param>
    /// <param name="channel">Channel where the message is from</param>
    /// <param name="reaction">Reaction that was removed</param>
    /// <returns></returns>
    private async Task OnReactionRemoved(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        // Reaction must come from a guild user and not the bot.
        if (reaction.User.Value is not SocketGuildUser user) return;
        if (reaction.User.Value.Id == _client.CurrentUser.Id) return;
        // Ensure message is from a guild channel.
        if (channel.Value is not SocketGuildChannel guildChannel) return;

        string emoteText = reaction.Emote.GetStringRepresentation();
        if (string.IsNullOrEmpty(emoteText)) return;

        string query = _api.Value.ReactionRolesUri
            .SetQueryParam("guildId", guildChannel.Guild.Id.ToString())
            .SetQueryParam("messageId", cachedMessage.Id.ToString());

        using var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<IEnumerable<ReactionRole>>(query);

        // Get reaction role.
        var reactionRole = response?
            .FirstOrDefault(r => r.Reaction == emoteText);

        // This reaction was not registered as a reaction role in the database.
        if (reactionRole is null) return;
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
            await channel.Value.SendMessageAsync(
                $"{e.Message}\nError removing role, make sure the role is lower than me in the server's roles list.");
            return;
        }

        _logger.LogTrace($"Removed role {role.Name} from user {user}");
    }

    /// <summary>
    ///     Remove all reaction roles from the database that referenced the message when all reactions from the message get
    ///     cleared.
    /// </summary>
    /// <param name="cachedMessage">Message whose reactions got cleared</param>
    /// <param name="channel">Channel in which the reactions of the message was cleared</param>
    /// <returns></returns>
    private async Task OnReactionsCleared(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> channel)
    {
        await ClearReactionRoles(cachedMessage.Id, channel);
    }

    /// <summary>
    ///     Remove all reactions to a specific emote from the database that referenced the message when its reactions was
    ///     cleared.
    /// </summary>
    /// <param name="cachedMessage">Message whose reaction got cleared</param>
    /// <param name="channel">Channel in which the reaction of the message was cleared</param>
    /// <param name="reaction">Reaction that was cleared</param>
    /// <returns></returns>
    private async Task OnReactionsRemovedForEmote(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> channel,
        IEmote reaction)
    {
        await ClearReactionRoles(cachedMessage.Id, channel, reaction);
    }

    /// <summary>
    ///     Remove reaction roles from the list and database when a guild deletes a role.
    ///     Clears reactions from all messages that referenced the deleted role.
    /// </summary>
    /// <param name="role">Deleted role</param>
    /// <returns></returns>
    private async Task OnRoleDeleted(SocketRole role)
    {
        // Construct query to send http request.
        string query = _api.Value.ReactionRolesUri
            .SetQueryParam("roleId", role.Id.ToString());

        using var client = _factory.CreateClient();
        var reactionRoles = await client.GetFromJsonAsync<List<ReactionRole>>(query);

        // No response for given query.
        if (reactionRoles is null)
        {
            return;
        }

        await client.DeleteAsync(query);
    }

    /// <summary>
    ///     When a message is deleted, all reaction roles that referenced that message
    ///     will get removed from the database and cleaned out of the list.
    /// </summary>
    /// <param name="cachedMessage">Deleted message</param>
    /// <param name="channel">Channel in which the message was deleted</param>
    /// <returns></returns>
    private async Task OnMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
    {
        await ClearReactionRoles(cachedMessage.Id, channel);
    }

    /// <summary>
    ///     Remove all reaction roles from the database for a specific reaction or all reactions from a message.
    ///     TODO: This method uses another DbContext when called from the other event handler methods.
    /// </summary>
    /// <param name="messageId">Id of the message to clear reactions from</param>
    /// <param name="channel">Channel in which the message is referenced</param>
    /// <param name="reaction">Specific reaction to clear from message. If none is specified, clear all reactions from message.</param>
    /// <returns></returns>
    private async Task ClearReactionRoles(ulong messageId, Cacheable<IMessageChannel, ulong> channel, IEmote? reaction = null)
    {
        // Ensure message is from a guild channel.
        if (channel.Value is not SocketGuildChannel guildChannel) return;

        // Construct query to send http request.
        string query = _api.Value.ReactionRolesUri
            .SetQueryParam("messageId", messageId.ToString());

        if (reaction is not null)
        {
            query.SetQueryParam("reaction", reaction.GetStringRepresentation());
        }

        using var client = _factory.CreateClient();
        var reactionRoles = await client.GetFromJsonAsync<List<ReactionRole>>(query);

        // No response for given query.
        if (reactionRoles is null)
        {
            return;
        }

        // This event is not related to reaction roles.
        if (reactionRoles.All(r =>
                r.GuildId == guildChannel.Guild.Id &&
                r.MessageId != messageId))
        {
            return;
        }

        await client.DeleteAsync(query);

        // // Condition to remove all reaction roles from a message if no reaction was specified,
        // // otherwise only remove all reaction roles for that specific reaction.
        // Expression<Func<ReactionRole, bool>> expression = reaction is null
        //     ? reactionRole => reactionRole.MessageId == messageId
        //     : reactionRole => reactionRole.MessageId == messageId &&
        //                       reactionRole.Reaction == reaction.GetStringRepresentation();
        //
        // // Get and remove all rows that referenced the message from database.
        // // var reactionRoles = await context.ReactionRoles.AsQueryable()
        // //     .Where(expression)
        // //     .ToListAsync();
        //
        // context.ReactionRoles.RemoveRange(reactionRoles);
        // try
        // {
        //     await context.SaveChangesAsync();
        //
        //     _logger.LogTrace(
        //         $"Removed {reactionRoles.Count.ToString()} reaction roles from message ({messageId.ToString()}), channel ({channel.Id.ToString()})");
        // }
        // catch
        // {
        //     _logger.LogError($"Error removing reaction roles from message ({messageId}), channel ({channel.Id})");
        //     throw;
        // }
    }
}
