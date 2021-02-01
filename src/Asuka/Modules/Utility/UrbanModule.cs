using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
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
            await ReplyAsync(query);
        }
    }
}
