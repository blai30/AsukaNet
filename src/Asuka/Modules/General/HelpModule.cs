using System.Linq;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General
{
    [Group("help")]
    [Alias("h", "halp")]
    [Remarks("General")]
    [Summary("View all commands or help info for a specific command.")]
    public class HelpModule : CommandModuleBase
    {
        private readonly CommandService _commandService;

        public HelpModule(
            IOptions<DiscordOptions> config,
            ILogger<HelpModule> logger,
            CommandService commandService) :
            base(config, logger)
        {
            _commandService = commandService;
        }

        [Command]
        [Remarks("help [command]")]
        public async Task HelpAsync(
            [Summary("Command name of which to view help info.")]
            ModuleInfo module = null)
        {
            // No module was specified, reply with default help embed.
            if (module is null)
            {
                await DefaultHelpAsync();
                return;
            }

            // List the module usage examples using the command remarks as usage if defined.
            string usage = module.Commands
                .Where(command => !string.IsNullOrWhiteSpace(command.Remarks))
                .Aggregate(string.Empty, (current, command) => $"{current}`{command.Remarks}`\n");

            // List of command aliases separated by a comma.
            var aliases = module.Aliases.Select(alias => $"`{alias}`");

            var embed = new EmbedBuilder()
                .WithTitle(module.Name)
                .WithDescription($"__Category: {module.Remarks}__\n{module.Summary}")
                .WithColor(Config.Value.EmbedColor)
                .AddField(
                    "Usage",
                    usage)
                .AddField(
                    "Aliases",
                    string.Join(", ", aliases))
                .Build();

            await ReplyAsync(embed: embed);
        }

        /// <summary>
        ///     Builds the default help embed for the bot that lists all available commands
        ///     separated by categories. Shows the user how to execute commands and provides
        ///     some useful links about the bot.
        /// </summary>
        /// <returns></returns>
        private async Task DefaultHelpAsync()
        {
            var clientUser = Context.Client.CurrentUser;
            string avatarUrl = clientUser.GetAvatarUrl();

            string[] links =
            {
                $"[Invite me]({Config.Value.InviteUrl})",
                $"[Invite me]({Config.Value.InviteUrl})"
            };

            // Initialize embed builder with basic info.
            var embed = new EmbedBuilder()
                .WithThumbnailUrl(avatarUrl)
                .WithDescription(
                    $"View available commands for {clientUser.Mention}.\n" +
                    $"Enter `@{clientUser.Username} help [command]` for command help info.")
                .WithAuthor(clientUser)
                .WithColor(Config.Value.EmbedColor)
                .AddField(
                    "Examples",
                    $"`@{clientUser.Username} help avatar` to view help for the avatar command.")
                .AddField(
                    "Useful links",
                    $"{string.Join(", ", links)}");

            // Get a sorted collection of command categories using
            // the module's remarks attribute as the category name.
            var moduleCategories = _commandService.Modules
                .Select(module => module.Remarks)
                .Distinct()
                .OrderBy(s => s);

            // Get a list of command names from each category.
            foreach (string category in moduleCategories)
            {
                // Skip any empty categories.
                if (string.IsNullOrWhiteSpace(category)) continue;

                // Get a sorted collection of command names,
                // wrapped in code block markdown.
                var commandList =
                    _commandService.Modules
                        .Where(module => module.Remarks == category)
                        .Select(module => $"`{module.Name}`")
                        .OrderBy(s => s);

                // Combine command names separated by a comma into a single string.
                string commands = string.Join(", ", commandList);
                embed.AddField(category, commands);
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
