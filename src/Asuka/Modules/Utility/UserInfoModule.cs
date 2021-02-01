using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    public class UserInfoModule : CommandModuleBase
    {
        public UserInfoModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command("userinfo")]
        [Summary("Display information about a user or self.")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user ??= Context.User;
            var avatarUrl = user.GetAvatarUrl();

            var embed = new EmbedBuilder()
                .WithTitle("Icon direct link")
                .WithUrl(avatarUrl)
                .WithAuthor(user.ToString(), avatarUrl)
                .WithColor(Config.Value.EmbedColor)
                .WithThumbnailUrl(avatarUrl)
                .WithFooter($"Created: {user.CreatedAt:R}")
                .AddField(
                    "ID",
                    user.Id,
                    true)
                .AddField(
                    "Bot",
                    user.IsBot,
                    true)
                .AddField(
                    "Presence",
                    user.Status,
                    true)
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
