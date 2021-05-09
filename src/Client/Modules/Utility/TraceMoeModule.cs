using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Models.Api.TraceMoe;
using Discord;
using Discord.Commands;
using Flurl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    [Group("tracemoe")]
    [Alias("whatanime")]
    [Remarks("Utility")]
    [Summary("Find out what anime the image came from.")]
    public class TraceMoeModule : CommandModuleBase
    {
        private const string Uri = "https://trace.moe";

        private readonly IHttpClientFactory _factory;

        public TraceMoeModule(
            IOptions<DiscordOptions> config,
            ILogger<TraceMoeModule> logger,
            IHttpClientFactory factory) :
            base(config, logger)
        {
            _factory = factory;
        }

        [Command]
        [Remarks("tracemoe <imageUrl>")]
        public async Task TraceMoeAsync(string? imageUrl = null)
        {
            string image = imageUrl ?? GetImageFromMessage();

            if (string.IsNullOrEmpty(image))
            {
                Logger.LogTrace($"Invalid image: {image}");
                await ReplyAsync("Give me a valid image!! (っ °Д °;)っ");
                return;
            }

            var responseBody = await GetTraceMoeResponse(image);

            // No response for given query.
            if (responseBody?.Docs is null)
            {
                return;
            }

            // Results are always sorted by best similarity so take first..
            var doc = responseBody.Docs.FirstOrDefault();

            // Similarity less than 87% is usually considered bad match, discard.
            if (doc is null || doc.Similarity < 0.87)
            {
                await ReplyAsync("Could not determine.");
                return;
            }

            var embed = GenerateEmbed(image, doc);

            await ReplyAsync(embed: embed);
        }

        /// <summary>
        ///     Generate the embed message with trade moe doc information.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="doc"></param>
        /// <returns>Embed message</returns>
        private Embed GenerateEmbed(string image, TraceMoeDoc doc)
        {
            string externalUrl = Uri
                .SetQueryParam("url", image);

            string description = doc.Synonyms is not null
                ? $"**Alternate titles:** {string.Join(", ", doc.Synonyms)}"
                : "No alternate titles.";

            var embed = new EmbedBuilder()
                .WithTitle(doc.TitleEnglish)
                .WithUrl(externalUrl)
                .WithColor(Config.Value.EmbedColor)
                .WithDescription(description)
                .WithFooter($"Traced from file: {doc.Filename}")
                .AddField(
                    "Romaji title",
                    doc.TitleRomaji,
                    true)
                .AddField(
                    "Native title",
                    doc.TitleNative,
                    true)
                .AddField(
                    "Chinese title",
                    doc.TitleChinese,
                    true)
                .AddField(
                    "AniList",
                    $"https://anilist.co/anime/{doc.AnilistId.ToString()}",
                    false)
                .AddField(
                    "MyAnimeList",
                    $"https://myanimelist.net/anime/{doc.MalId.ToString()}",
                    false)
                .AddField(
                    "For 18+",
                    doc.IsAdult ? "Yes" : "No",
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
                .AppendPathSegment("api")
                .AppendPathSegment("search")
                .SetQueryParam("url", imageUrl);

            // Send request and get JSON response.
            using var client = _factory.CreateClient();
            var responseBody = await client.GetFromJsonAsync<TraceMoeResponse>(query);
            return responseBody;
        }

        /// <summary>
        ///     Get image url from first message attachment.
        /// </summary>
        /// <returns>Image url</returns>
        private string GetImageFromMessage()
        {
            return Context.Message.Attachments.First().Url;
        }
    }
}
