using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    public class UrbanModule : CommandModuleBase
    {
        public UrbanModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command("urban")]
        [Summary("Look up a word or phrase on Urban Dictionary.")]
        public async Task UrbanAsync([Remainder] string term)
        {
            var url = "https://api.urbandictionary.com/v0/define";
            var query = QueryHelpers.AddQueryString(url, "term", term);

            var embed = new EmbedBuilder()
                .WithTitle("entry.word")
                .WithUrl("https://www.google.com/")
                .WithColor(0xefff00)
                .WithAuthor("Urban Dictionary", Context.Guild.IconUrl)
                .WithDescription("Written on: parse date entry.written_on")
                .WithFooter(
                    "👍 entry.thumbsUp\n" +
                    "👎 entry.thumbsDown")
                .AddField(
                    "Definition",
                    "entry.definition trim to 1024 char")
                .AddField(
                    "Example",
                    "entry.example trim to 1024 char")
                .AddField(
                    "Author",
                    "entry.author")
                .Build();

            await ReplyAsync(query, embed: embed);
        }
    }
}
