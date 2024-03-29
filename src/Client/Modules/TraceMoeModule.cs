﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Asuka.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Flurl;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules;

public class TraceMoeModule : InteractionModule
{
    private const string Uri = "https://api.trace.moe";

    private readonly IHttpClientFactory _factory;

    public TraceMoeModule(
        IOptions<DiscordOptions> config,
        ILogger<TraceMoeModule> logger,
        IHttpClientFactory factory) :
        base(config, logger)
    {
        _factory = factory;
    }

    [SlashCommand(
        "tracemoe",
        "Find out what anime the image came from.")]
    public async Task TraceMoeAsync(string imageUrl)
    {
        bool success = await RunTraceMoe(imageUrl);

        if (success)
        {
            await ReplyAsync(imageUrl);
        }
    }

    [MessageCommand("What anime?")]
    public async Task TraceMoeAsync(SocketMessage message)
    {
        string imageUrl = message.Attachments.FirstOrDefault()?.Url ?? message.ToString();
        await RunTraceMoe(imageUrl);
    }

    private async Task<bool> RunTraceMoe(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.LogTrace($"Invalid image: {imageUrl}");
            await RespondAsync("Give me a valid image!! (っ °Д °;)っ", ephemeral: true);
            return false;
        }

        await DeferAsync();
        var responseBody = await GetTraceMoeResponse(imageUrl);

        // No response for given query.
        if (responseBody?.Result is null)
        {
            await Context.Interaction
                .ModifyOriginalResponseAsync(properties => properties.Content = "Command failed.");
            return false;
        }

        // Results are always sorted by best similarity so take first..
        var doc = responseBody.Result.FirstOrDefault();

        // Similarity less than 87% is usually considered bad match, discard.
        if (doc is null || doc.Similarity < 0.87)
        {
            await Context.Interaction
                .ModifyOriginalResponseAsync(properties => properties.Content = "Could not determine.");
            return false;
        }

        var coverImageUrl = await GetAnilistCoverImage(doc.Anilist?.Id);
        var embed = GenerateEmbed(imageUrl, doc, coverImageUrl);
        await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Embed = embed);
        return true;
    }

    /// <summary>
    ///     Generate the embed message with trade moe doc information.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="doc"></param>
    /// <param name="coverImageUrl"></param>
    /// <returns>Embed message</returns>
    private Embed GenerateEmbed(string image, Result doc, Uri? coverImageUrl)
    {
        string externalUrl = "https://trace.moe"
            .SetQueryParam("url", image);

        string description = doc.Anilist?.Synonyms is not null
            ? $"**Alternate title(s):** {string.Join(", ", doc.Anilist.Synonyms)}"
            : "No alternate titles.";

        var embed = new EmbedBuilder()
            .WithTitle(doc.Anilist?.Title?.English ?? "No title")
            .WithUrl(externalUrl)
            .WithDescription(description)
            .WithColor(Config.Value.EmbedColor)
            .WithThumbnailUrl(coverImageUrl?.ToString())
            .WithFooter($"Traced from file: {doc.Filename}")
            .AddField(
                "Romaji title",
                doc.Anilist?.Title?.Romaji ?? "N/A",
                true)
            .AddField(
                "Native title",
                doc.Anilist?.Title?.Native ?? "N/A",
                true)
            .AddField(
                "English title",
                doc.Anilist?.Title?.English ?? "N/A",
                true)
            .AddField(
                "AniList",
                $"https://anilist.co/anime/{doc.Anilist?.Id.ToString()}",
                false)
            .AddField(
                "MyAnimeList",
                $"https://myanimelist.net/anime/{doc.Anilist?.IdMal.ToString()}",
                false)
            .AddField(
                "For 18+",
                doc.Anilist?.IsAdult is null ? "Unknown" : doc.Anilist?.IsAdult ?? false ? "Yes" : "No",
                true)
            .Build();

        return embed;
    }

    /// <summary>
    ///     Send request to API and get response using image url.
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns>TraceMoeResponse</returns>
    private async Task<TraceMoeResponse?> GetTraceMoeResponse(string imageUrl)
    {
        // Construct query to send http request.
        string query = Uri
            .AppendPathSegment("search")
            .SetQueryParam("anilistInfo")
            .SetQueryParam("cutBorders")
            .SetQueryParam("url", imageUrl);

        Logger.LogInformation(query);

        // Send request and get JSON response.
        using var client = _factory.CreateClient();
        try
        {
            var responseBody = await client.GetFromJsonAsync<TraceMoeResponse>(query);
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e.ToString());
            return null;
        }
    }

    private async Task<Uri?> GetAnilistCoverImage(ulong? anilistId)
    {
        var graphQlClient = new GraphQLHttpClient(
            "https://graphql.anilist.co",
            new SystemTextJsonSerializer());

        var request = new GraphQLRequest(@"
            query ($id: Int) {
                Media (id: $id) {
                    coverImage {
                        large
                    }
                }
            }
        ", new { id = anilistId });

        try
        {
            var response = await graphQlClient.SendQueryAsync<AnilistResponse>(request);
            return response.Data.Media?.CoverImage?.Large;
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e.ToString());
            return null;
        }
    }
}
