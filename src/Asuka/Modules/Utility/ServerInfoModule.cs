using System.Linq;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    public class ServerInfoModule : CommandModuleBase
    {
        public ServerInfoModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command("serverinfo")]
        [Summary("Display information about the server.")]
        public async Task ServerInfoAsync()
        {
            var guild = Context.Guild;
            var textChannels = guild.TextChannels;
            var voiceChannels = guild.VoiceChannels;
            var emotes = guild.Emotes;
            var roles = guild.Roles;
            var guildIconUrl = guild.IconUrl;

            var roleList = roles.Select(role => role.Name).ToList();
            roleList.Sort();

            var embed = new EmbedBuilder()
                .WithTitle("Icon direct link")
                .WithUrl(guildIconUrl)
                .WithAuthor(guild.Name, guildIconUrl)
                .WithDescription($"Owner: {guild.Owner.Mention}")
                .WithColor(Config.Value.EmbedColor)
                .WithThumbnailUrl(guildIconUrl)
                .WithFooter($"Created: {guild.CreatedAt:R}")
                .AddField(
                    "ID",
                    guild.Id,
                    true)
                .AddField(
                    "Region",
                    guild.VoiceRegionId,
                    true)
                .AddField(
                    "Members",
                    guild.MemberCount,
                    true)
                .AddField(
                    "Channels",
                    $"{textChannels.Count} text\n" +
                    $"{voiceChannels.Count} voice",
                    true)
                .AddField(
                    "Emotes",
                    $"{emotes.Select(emote => !emote.Animated).Count()} static\n" +
                    $"{emotes.Select(emote => emote.Animated).Count()} animated",
                    true)
                .AddField(
                    "Premium",
                    $"Level {(int) guild.PremiumTier}\n" +
                    $"{guild.PremiumSubscriptionCount} boosts",
                    true)
                .AddField(
                    "Roles",
                    string.Join(", ", roleList))
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
