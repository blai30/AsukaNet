using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Models.Api.Urban;
using Discord;
using Discord.Commands;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    [Group("urban")]
    [Remarks("Utility")]
    [Summary("Look up a word or phrase on Urban Dictionary.")]
    public class UrbanModule : CommandModuleBase
    {
        private readonly HttpClient _client;

        private const string Uri = "https://api.urbandictionary.com/v0/define";

        public UrbanModule(
            IOptions<DiscordOptions> config,
            HttpClient client)
            : base(config)
        {
            _client = client;
        }

        [Command]
        [Name("")]
        public async Task UrbanAsync([Remainder] string term)
        {
            // Build query and send http request to urban api.
            var query = QueryHelpers.AddQueryString(Uri, "term", term);
            var streamTask = _client.GetStreamAsync(query);

            // Deserialize json response.
            var results = await JsonSerializer.DeserializeAsync<UrbanList>(await streamTask);

            // Term did not yield results.
            if (results?.UrbanEntries == null || results.UrbanEntries.Count <= 0)
            {
                await ReplyAsync($"`{term.Truncate(32, true)}` was not found in the Urban Dictionary.");
                return;
            }

            var entry = results.UrbanEntries[0];

            var embed = new EmbedBuilder()
                .WithTitle(entry.Word)
                .WithUrl(entry.Permalink)
                .WithColor(0xefff00)
                .WithAuthor("Urban Dictionary")
                .WithDescription($"Written on: {entry.WrittenOn:R}")
                .WithFooter(
                    $"👍 {entry.ThumbsUp}\n" +
                    $"👎 {entry.ThumbsDown}")
                .AddField(
                    "Definition",
                    entry.Definition.Truncate(1024, true))
                .AddField(
                    "Example",
                    entry.Example.Truncate(1024, true))
                .AddField(
                    "Author",
                    entry.Author)
                .Build();

            await ReplyAsync(query, embed: embed);
        }
    }
}
