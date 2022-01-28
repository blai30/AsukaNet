using System.Linq;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General;

public class HelpModule : InteractionModule
{
    private readonly InteractionService _interactionService;

    public HelpModule(
        IOptions<DiscordOptions> config,
        ILogger<HelpModule> logger,
        InteractionService interactionService) :
        base(config, logger)
    {
        _interactionService = interactionService;
    }

    /// <summary>
    ///     Builds the default help embed for the bot that lists all available commands
    ///     separated by categories. Shows the user how to execute commands and provides
    ///     some useful links about the bot.
    /// </summary>
    /// <returns></returns>
    [SlashCommand(
        "help",
        "View all available commands.")]
    public async Task HelpAsync()
    {
        var clientUser = Context.Client.CurrentUser;
        string avatarUrl = clientUser.GetAvatarUrl();

        (string, string)[] links =
        {
            ("Invite", Config.Value.InviteUrl),
            ("GitHub", Config.Value.GitHubUrl)
        };

        // Initialize embed builder with basic info.
        var embed = new EmbedBuilder()
            .WithThumbnailUrl(avatarUrl)
            .WithDescription($"View available commands for {clientUser.Mention}.\n")
            .WithAuthor(clientUser)
            .WithColor(Config.Value.EmbedColor);

        // Add list of commands to embed.
        var slashCommands = _interactionService.Modules
            .SelectMany(module => module.SlashCommands);

        var commandsList = slashCommands
            .OrderBy(command => command.Name)
            .Select(command => $"`/{command.Name}`");

        embed.AddField("Commands", string.Join("\n", commandsList));

        // Add button to component builder for each link.
        var componentBuilder = new ComponentBuilder();
        foreach ((string label, string url) in links)
        {
            componentBuilder.WithButton(label, style: ButtonStyle.Link, url: url);
        }

        await RespondAsync(embed: embed.Build(), components: componentBuilder.Build());
    }
}
