using System.Linq;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General;

[RequireContext(ContextType.Guild)]
public class ServerInfoModule : InteractionModule
{
    public ServerInfoModule(
        IOptions<DiscordOptions> config,
        ILogger<ServerInfoModule> logger) :
        base(config, logger)
    {
    }

    [SlashCommand(
        "serverinfo",
        "Display information about the server.")]
    public async Task ServerInfoAsync()
    {
        // Collect guild information.
        var guild = Context.Guild;
        var textChannels = guild.TextChannels;
        var voiceChannels = guild.VoiceChannels;
        var emotes = guild.Emotes;
        var roles = guild.Roles;
        string guildIconUrl = guild.IconUrl;
        int staticEmotesCount = emotes.Count(emote => !emote.Animated);
        int animatedEmotesCount = emotes.Count(emote => emote.Animated);

        // Sort collection of roles to print alphabetically.
        var rolesList = roles.Select(role => role.Name).ToList();
        rolesList.Sort();

        var regionsList = await guild.GetVoiceRegionsAsync();
        var regions = regionsList.Select(region => region.Name);

        var embed = new EmbedBuilder()
            .WithAuthor(guild.Name, guildIconUrl)
            .WithColor(Config.Value.EmbedColor)
            .WithImageUrl(guild.BannerUrl)
            .WithThumbnailUrl(guildIconUrl)
            .WithFooter($"Created: {guild.CreatedAt.ToString("R")}")
            .AddField(
                "Voice regions",
                string.Join(", ", regions))
            .AddField(
                "ID",
                guild.Id.ToString(),
                true)
            .AddField(
                "Max Bitrate",
                $"{guild.MaxBitrate.ToString()} kbps",
                true)
            .AddField(
                "Members",
                guild.MemberCount.ToString(),
                true)
            .AddField(
                "Channels",
                $"{textChannels.Count.ToString()} text\n{voiceChannels.Count.ToString()} voice",
                true)
            .AddField(
                "Emotes",
                $"{staticEmotesCount.ToString()} static\n{animatedEmotesCount.ToString()} animated",
                true)
            .AddField(
                "Premium",
                $"Level {((int) guild.PremiumTier).ToString()}\n{guild.PremiumSubscriptionCount.ToString()} boosts",
                true)
            .AddField(
                "Roles",
                string.Join(", ", rolesList))
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Icon direct link", style: ButtonStyle.Link, url: guildIconUrl)
            .Build();

        await RespondAsync(embed: embed, components: components);
    }
}
