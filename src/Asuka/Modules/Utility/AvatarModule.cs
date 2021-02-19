using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    [Group("avatar")]
    [Remarks("Utility")]
    [Summary("Displays the avatar of a user or self.")]
    public class AvatarModule : CommandModuleBase
    {
        public AvatarModule(
            IOptions<DiscordOptions> config,
            ILogger<AvatarModule> logger) :
            base(config, logger)
        {
        }

        [Command]
        [Remarks("avatar [@user]")]
        public async Task AvatarAsync(IUser user = null)
        {
            user ??= Context.User;
            string avatarUrl = user.GetAvatarUrl(size: 4096);

            // Construct embed with avatar image and direct link.
            var embed = new EmbedBuilder()
                .WithTitle("Direct link")
                .WithUrl(avatarUrl)
                .WithImageUrl(avatarUrl)
                .WithAuthor(user)
                .WithColor(Config.Value.EmbedColor)
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
