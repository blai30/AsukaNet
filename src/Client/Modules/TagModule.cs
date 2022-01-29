﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Asuka.Models.Api.Asuka;
using Discord;
using Discord.Interactions;
using Flurl;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules;

[Group(
    "tag",
    "Add, edit, or delete tags.")]
[RequireContext(ContextType.Guild)]
public class TagModule : InteractionModule
{
    private readonly IOptions<ApiOptions> _api;
    private readonly IHttpClientFactory _factory;

    public TagModule(
        IOptions<ApiOptions> api,
        IOptions<DiscordOptions> config,
        ILogger<TagModule> logger,
        IHttpClientFactory factory) :
        base(config, logger)
    {
        _api = api;
        _factory = factory;
    }

    [SlashCommand(
        "add",
        "Create a new tag for the server.")]
    public async Task AddAsync(string tagName, string tagContent, IEmote? reaction = null)
    {
        if (tagName.Length > 100)
        {
            await RespondAsync("Tag name must be 100 characters or less.");
            return;
        }

        if (tagContent.Length > 255)
        {
            await RespondAsync("Tag content must be 255 characters or less.");
            return;
        }

        var tag = new Tag
        {
            Name = tagName,
            Content = tagContent,
            Reaction = reaction?.ToString(),
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id
        };

        // Send post request to api using json body.
        string json = JsonSerializer.Serialize(tag);
        using var client = _factory.CreateClient();

        try
        {
            await client.PostAsync(_api.Value.TagsUri, new StringContent(json, Encoding.UTF8, "application/json"));
            await RespondAsync($"Added new tag `{tag.Name}`.");
        }
        catch
        {
            await RespondAsync(
                $"Error adding `{tagName.Truncate(20, "...")}`, either a tag with the same name already exists or the input parameters are invalid.");
        }
    }

    [SlashCommand(
        "remove",
        "Remove a tag from the server.")]
    public async Task RemoveAsync(string tagName)
    {
        var tag = await GetTagByName(tagName);

        // Tag does not exist.
        if (tag is null)
        {
            await RespondAsync($"Tag `{tagName.Truncate(20, "...")}` does not exist in this server.");
            return;
        }

        // Send delete request to api using id.
        string command = _api.Value.TagsUri.AppendPathSegment(tag.Id.ToString());
        using var client = _factory.CreateClient();

        try
        {
            await client.DeleteAsync(command);
            await RespondAsync($"Removed tag `{tag.Name}`.");
        }
        catch
        {
            await RespondAsync($"Error removing tag `{tagName.Truncate(20, "...")}`.");
            throw;
        }
    }

    [SlashCommand(
        "edit",
        "Edit an existing tag from the server.")]
    public async Task EditAsync(string tagName, string tagContent, IEmote? reaction = null)
    {
        var tag = await GetTagByName(tagName);

        // Tag does not exist.
        if (tag is null)
        {
            await RespondAsync($"Tag `{tagName.Truncate(20, "...")}` does not exist in this server.");
            return;
        }

        string json = JsonSerializer.Serialize(new Tag
        {
            Id = tag.Id,
            Content = tagContent,
            Reaction = reaction?.ToString()
        });

        // Send put request to api using json body.
        string command = _api.Value.TagsUri.AppendPathSegment("edit");
        using var client = _factory.CreateClient();

        try
        {
            await client.PutAsync(command, new StringContent(json, Encoding.UTF8, "application/json"));
            await RespondAsync($"Updated tag `{tag.Name}` with content `{tag.Content}`.");
        }
        catch
        {
            await RespondAsync($"Error updating tag `{tagName.Truncate(20, "...")}`.");
            throw;
        }
    }

    [SlashCommand(
        "list",
        "List all tags from the server.")]
    public async Task ListAsync()
    {
        // Get list of tags from this guild.
        string query = _api.Value.TagsUri
            .SetQueryParam("guildId", Context.Guild.Id.ToString());

        using var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<IEnumerable<Tag>>(query);

        if (response is null)
        {
            Logger.LogTrace($"Error getting list of tags for guild with id: {Context.Guild.Id.ToString()}");
            await RespondAsync("No tags found for this server.");
            return;
        }

        var tags = response
            .Select(t => $"`{t.Name}`")
            .ToList();

        // Join list of tag names with comma.
        string list = string.Join(", ", tags);
        await RespondAsync($"Tags: {list}");
    }

    [SlashCommand(
        "info",
        "Show info for a tag from the server.")]
    public async Task InfoAsync(string tagName)
    {
        var tag = await GetTagByName(tagName);

        // Tag does not exist.
        if (tag is null)
        {
            await RespondAsync($"Tag `{tagName.Truncate(20, "...")}` does not exist in this server.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle(tag.Name)
            .WithColor(Config.Value.EmbedColor)
            .WithDescription(tag.Content)
            .AddField("Added by", Context.Client.GetUser(tag.UserId).Mention)
            .AddField("Usage count", tag.UsageCount.ToString())
            .AddField("Created", tag.CreatedAt.GetValueOrDefault().ToString("R"))
            .AddField("Last used", tag.UpdatedAt.GetValueOrDefault().ToString("R"))
            .Build();

        await RespondAsync(embed: embed);
    }

    private async Task<Tag?> GetTagByName(string tagName)
    {
        // Get list of tags from this guild.
        string query = _api.Value.TagsUri
            .SetQueryParam("name", tagName)
            .SetQueryParam("guildId", Context.Guild.Id.ToString());

        // Send get request to api using query parameters for tag name and guild id.
        using var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<IEnumerable<Tag>>(query);
        var tag = response?.FirstOrDefault(t =>
            t.Name == tagName &&
            t.GuildId == Context.Guild.Id);

        return tag;
    }
}