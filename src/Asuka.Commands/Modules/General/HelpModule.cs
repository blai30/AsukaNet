using System.Linq;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Commands.Modules.General
{
    [Group("help")]
    [Alias("h", "halp")]
    [Remarks("General")]
    [Summary("View all commands or help info for a specific command.")]
    public class HelpModule : CommandModuleBase
    {
        private CommandService _commandService;

        public HelpModule(
            IOptions<DiscordOptions> config,
            CommandService commandService)
            : base(config)
        {
            _commandService = commandService;
        }

        [Command]
        [Remarks("help [command]")]
        public async Task HelpAsync(
            [Summary("Command name of which to view help info.")]
            string commandName)
        {
            // Get module by name or alias.
            var module = _commandService.Modules
                .FirstOrDefault(moduleInfo =>
                    moduleInfo.Name == commandName ||
                    moduleInfo.Aliases.Contains(commandName));

            // Specified module is invalid.
            if (module == null)
            {
                await ReplyInlineAsync($"Command `{commandName}` does not exist.");
                return;
            }

            // List the module usage examples using the command remarks.
            string usage = string.Empty;
            foreach (var command in module.Commands)
            {
                // Use command remarks as usage.
                if (string.IsNullOrWhiteSpace(command.Remarks)) continue;
                usage += $"`{command.Remarks}`\n";
            }

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

        [Command]
        public async Task HelpAsync()
        {
            var clientUser = Context.Client.CurrentUser;
            var avatarUrl = clientUser.GetAvatarUrl();

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
            var moduleCategories =
                _commandService.Modules
                    .Select(module => module.Remarks)
                    .Distinct()
                    .OrderBy(s => s);

            // Get a list of command names from each category.
            foreach (var category in moduleCategories)
            {
                // Skip any empty categories.
                if (string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                // Get a sorted collection of command names,
                // wrapped in code block markdown.
                var commandList =
                    _commandService.Modules
                        .Where(module => module.Remarks == category)
                        .Select(module => $"`{module.Name}`")
                        .OrderBy(s => s);

                // Combine command names separated by a comma into a single string.
                var commands = string.Join(", ", commandList);
                embed.AddField(category, commands);
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
