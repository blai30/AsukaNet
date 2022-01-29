using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules;

public class AvatarModule : InteractionModule
{
    public AvatarModule(
        IOptions<DiscordOptions> config,
        ILogger<AvatarModule> logger) :
        base(config, logger)
    {
    }

    [SlashCommand(
        "avatar",
        "Displays the avatar of a user or self.")]
    [UserCommand("Get avatar")]
    public async Task AvatarAsync(
        [Summary(description: "User from which to get avatar.")] IUser? user = null)
    {
        user ??= Context.User;
        string avatarUrl = user.GetAvatarUrl(size: 4096);

        // Construct message with avatar image and direct link.
        var embed = new EmbedBuilder()
            .WithImageUrl(avatarUrl)
            .WithAuthor(user)
            .WithColor(Config.Value.EmbedColor)
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Direct link", style: ButtonStyle.Link, url: avatarUrl)
            .Build();

        await RespondAsync(embed: embed, components: components);
    }
}
