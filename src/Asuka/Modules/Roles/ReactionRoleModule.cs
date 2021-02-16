using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Database;
using Asuka.Database.Models;
using Asuka.Services;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
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
        private readonly IDbContextFactory<AsukaDbContext> _factory;
        private readonly ReactionRoleService _service;

        public ReactionRoleModule(
            IOptions<DiscordOptions> config,
            IDbContextFactory<AsukaDbContext> factory,
            ReactionRoleService service) :
            base(config)
        {
            _factory = factory;
            _service = service;
        }

        [Command("create")]
        [Alias("c")]
        [Remarks("reactionrole create")]
        [Summary("Create a reaction role message.")]
        public async Task CreateAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("React to receive roles")
                .WithColor(Config.Value.EmbedColor)
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("add")]
        [Alias("a")]
        [Remarks("reactionrole add <messageId> <:emoji:> <@role>")]
        [Summary("Add a reaction role to a reaction role message.")]
        public async Task AddAsync(IMessage message, IEmote emote, IRole role)
        {
            if (message == null)
            {
                await ReplyAsync("Reaction role message not found.");
                return;
            }

            // Get emote string representation and guild role by role id.
            string emoteText = emote.GetStringRepresentation();
            var guildRole = Context.Guild.GetRole(role.Id);

            var reactionRole = new ReactionRole
            {
                GuildId = Context.Guild.Id,
                MessageId = message.Id,
                RoleId = guildRole.Id,
                Emote = emoteText
            };

            // Add reaction role to database.
            await using var context = _factory.CreateDbContext();
            await context.ReactionRoles.AddAsync(reactionRole);
            try
            {
                await context.SaveChangesAsync();
                await message.AddReactionAsync(emote);
                _service.ReactionRoles.Add(reactionRole);
                await ReplyAsync($"Added reaction role {guildRole.Mention}.");
            }
            catch
            {
                await ReplyAsync("Could not add reaction role.");
                throw;
            }
        }

        [Command("remove")]
        [Alias("r")]
        [Remarks("reactionrole remove <messageId> <@role>")]
        [Summary("Remove a reaction role from a reaction role message.")]
        public async Task RemoveAsync(IMessage message, IRole role)
        {
        }
    }
}
