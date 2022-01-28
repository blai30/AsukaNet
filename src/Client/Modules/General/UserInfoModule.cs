using System;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General;

public class UserInfoModule : InteractionModule
{
    public UserInfoModule(
        IOptions<DiscordOptions> config,
        ILogger<UserInfoModule> logger) :
        base(config, logger)
    {
    }

    [SlashCommand(
        "userinfo",
        "Display information about a user or self.")]
    [UserCommand("Get user info")]
    public async Task UserInfoAsync(IUser? user = null)
    {
        // Use self if no user was specified.
        user ??= Context.User;
        string avatarUrl = user.GetAvatarUrl();

        var embed = new EmbedBuilder()
            .WithTitle("User info")
            .WithAuthor(user.ToString(), avatarUrl)
            .WithColor(Config.Value.EmbedColor)
            .WithThumbnailUrl(avatarUrl)
            .WithFooter($"Created: {user.CreatedAt.ToString("R")}")
            .AddField(
                "ID",
                user.Id.ToString(),
                true)
            .AddField(
                "Bot",
                user.IsBot.ToString(),
                true)
            .AddField(
                "Presence",
                Enum.GetName(user.Status),
                true)
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Avatar direct link", style: ButtonStyle.Link, url: avatarUrl)
            .Build();

        await RespondAsync(embed: embed, components: components);
    }
}
