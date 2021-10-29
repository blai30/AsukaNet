using System;
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

public class TagListenerService : IHostedService
{
    private readonly IOptions<ApiOptions> _api;
    private readonly DiscordSocketClient _client;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<TagListenerService> _logger;

    public TagListenerService(
        IOptions<ApiOptions> api,
        DiscordSocketClient client,
        IHttpClientFactory factory,
        ILogger<TagListenerService> logger)
    {
        _api = api;
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

        string query = _api.Value.TagsUri
            .SetQueryParam("name", message.Content)
            .SetQueryParam("guildId", guildChannel.Guild.Id.ToString());

        using var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<IEnumerable<Tag>>(query);

        var tag = response?.FirstOrDefault(t =>
            t.Name == message.Content &&
            t.GuildId == guildChannel.Guild.Id);

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
