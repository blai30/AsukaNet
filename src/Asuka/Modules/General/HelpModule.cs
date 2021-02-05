using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General
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
        [Name("")]
        public async Task HelpAsync(
            [Summary("Command name of which to view help info.")]
            string commandName)
        {
            var module = _commandService.Modules.First(moduleInfo => moduleInfo.Name == commandName);

            var usage = new StringBuilder();
            foreach (var command in module.Commands)
            {
                usage.Append(module.Name);
                usage.Append(" " + command.Name);
                foreach (var parameter in command.Parameters)
                {
                    usage.Append(
                        parameter.IsOptional ?
                            $" [{parameter.Name}] " :
                            $" <{parameter.Name}>");
                }

                usage.AppendLine();
            }

            var aliases = module.Aliases.Select(alias => $"`{alias}`");

            var embed = new EmbedBuilder()
                .WithTitle(module.Name)
                .WithDescription($"__{module.Remarks}__\n{module.Summary}")
                .WithColor(Config.Value.EmbedColor)
                .AddField("Usage", $"`{usage}`")
                .AddField("Aliases", string.Join(", ", aliases))
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command]
        [Name("")]
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
                .WithColor(0xe91e63)
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
