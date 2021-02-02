using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Roles
{
    [Group("reactionrole")]
    [Alias("rr")]
    [RequireContext(ContextType.Guild)]
    public class ReactionRoleModule : CommandModuleBase
    {
        public ReactionRoleModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command("create")]
        [Alias("c")]
        [Summary("Create a reaction role message.")]
        public async Task CreateRoleAsync()
        {
            await Task.CompletedTask;
        }

        [Command("add")]
        [Alias("a")]
        [Summary("Add a reaction role to a reaction role message.")]
        public async Task AddRoleAsync()
        {
            await Task.CompletedTask;
        }

        [Command("remove")]
        [Alias("r")]
        [Summary("Remove a reaction role from a reaction role message.")]
        public async Task RemoveRoleAsync()
        {
            await Task.CompletedTask;
        }
    }
}
