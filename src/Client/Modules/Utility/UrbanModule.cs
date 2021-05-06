using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Models.API.Urban;
using Discord;
using Discord.Commands;
using Flurl;
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
        private const string Uri = "https://api.urbandictionary.com/v0";

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
            // Construct query to send http request.
            string query = Uri
                .AppendPathSegment("define")
                .SetQueryParam("term", term);

            // Send request and get JSON response.
            using var client = _factory.CreateClient();
            var responseBody = await client.GetFromJsonAsync<UrbanList>(query);

            // No response for given query.
            if (responseBody?.UrbanEntries is null || responseBody.UrbanEntries.Count <= 0)
            {
                await ReplyAsync($"`{term.Truncate(32, "...")}` was not found in the Urban Dictionary.");
                return;
            }

            // Take the first result.
            var entry = responseBody.UrbanEntries[0];

            var embed = new EmbedBuilder()
                .WithTitle(entry.Word)
                .WithUrl(entry.Permalink)
                .WithColor(0xefff00)
                .WithAuthor("Urban Dictionary")
                .WithDescription($"Written on: {entry.WrittenOn.ToString("R")}")
                .WithFooter(
                    $"👍 {entry.ThumbsUp.ToString()}\n👎 {entry.ThumbsDown.ToString()}")
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
