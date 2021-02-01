using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    public class AvatarModule : CommandModuleBase
    {
        private readonly IOptions<DiscordOptions> _config;

        public AvatarModule(IOptions<DiscordOptions> config)
        {
            _config = config;
        }

        [Command("avatar")]
        public async Task AvatarAsync(IUser user = null)
        {
            user ??= Context.User;
            var avatarUrl = user.GetAvatarUrl(size: 4096);

            // Construct embed with avatar image and direct link.
            var embed = new EmbedBuilder()
                .WithTitle("Direct link")
                .WithUrl(avatarUrl)
                .WithImageUrl(avatarUrl)
                .WithAuthor(user)
                .WithColor(_config.Value.EmbedColor)
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
