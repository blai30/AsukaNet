using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Models.API.Urban;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    [Group("urban")]
    [Remarks("Utility")]
    [Summary("Look up a word or phrase on Urban Dictionary.")]
    public class UrbanModule : CommandModuleBase
    {
        private const string Uri = "https://api.urbandictionary.com/v0/define";

        private readonly IHttpClientFactory _factory;

        public UrbanModule(
            IOptions<DiscordOptions> config,
            ILogger<UrbanModule> logger,
            IHttpClientFactory factory) :
            base(config, logger)
        {
            _factory = factory;
        }

        [Command]
        [Remarks("urban <term>")]
        public async Task UrbanAsync([Remainder] string term)
        {
            // Build query and send http request to urban api.
            using var client = _factory.CreateClient();
            var builder = new UriBuilder(Uri)
            {
                Query = $"term={term}"
            };
            var results = await client.GetFromJsonAsync<UrbanList>(builder.Uri);

            // No entry found for given query.
            if (results?.UrbanEntries == null || results.UrbanEntries.Count <= 0)
            {
                await ReplyAsync($"`{term.Truncate(32, "...")}` was not found in the Urban Dictionary.");
                return;
            }

            // Take the first result.
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
                    entry.Definition.Truncate(1024, "..."))
                .AddField(
                    "Example",
                    entry.Example.Truncate(1024, "..."))
                .AddField(
                    "Author",
                    entry.Author)
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
