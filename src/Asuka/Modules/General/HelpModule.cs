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
        public async Task HelpAsync()
        {
            var clientUser = Context.Client.CurrentUser;
            var avatarUrl = clientUser.GetAvatarUrl();
            string[] links =
            {
                $"[Invite me]({Config.Value.InviteUrl})",
                $"[Invite me]({Config.Value.InviteUrl})"
            };

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
                    $"{string.Join(", ", links)}")
                .Build();

            await ReplyAsync(embed: embed);
        }

        // [Command]
        // [Alias("h", "halp")]
        // [Summary("View all commands or help info for a specific command.")]
        // public async Task HelpAsync(
        //     [Summary("Command name of which to view help info.")]
        //     string commandName)
        // {
        //     await ReplyAsync(string.Join(", ", CommandService.Commands.Select(c => c.Name)));
        // }
    }
}
