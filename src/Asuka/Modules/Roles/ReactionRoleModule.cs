using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Services;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Roles
{
    [Group("reactionrole")]
    [Alias("rr")]
    [Remarks("Roles")]
    [Summary("Manage reaction roles for the server.")]
    [RequireBotPermission(
        ChannelPermission.AddReactions |
        ChannelPermission.ManageMessages |
        ChannelPermission.ManageRoles |
        ChannelPermission.ReadMessageHistory |
        ChannelPermission.ViewChannel)]
    [RequireUserPermission(
        ChannelPermission.AddReactions |
        ChannelPermission.ManageMessages |
        ChannelPermission.ManageRoles |
        ChannelPermission.ReadMessageHistory |
        ChannelPermission.ViewChannel)]
    [RequireContext(ContextType.Guild)]
    public class ReactionRoleModule : CommandModuleBase
    {
        private readonly ReactionRoleService _service;

        public ReactionRoleModule(
            IOptions<DiscordOptions> config,
            IServiceScopeFactory scopeFactory) :
            base(config)
        {
            using var scope = scopeFactory.CreateScope();
            _service = scope.ServiceProvider.GetService<ReactionRoleService>();
        }

        [Command("create")]
        [Alias("c")]
        [Summary("Create a reaction role message.")]
        [Remarks("reactionrole create")]
        public async Task CreateRoleAsync()
        {
            await Task.CompletedTask;
        }

        [Command("add")]
        [Alias("a")]
        [Summary("Add a reaction role to a reaction role message.")]
        [Remarks("reactionrole add <messageId> <:emoji:> <@role>")]
        public async Task AddRoleAsync()
        {
            await Task.CompletedTask;
        }

        [Command("remove")]
        [Alias("r")]
        [Summary("Remove a reaction role from a reaction role message.")]
        [Remarks("reactionrole remove <messageId> <@role>")]
        public async Task RemoveRoleAsync()
        {
            await Task.CompletedTask;
        }
    }
}
