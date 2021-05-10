using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Models.Api.Asuka;
using Discord;
using Discord.Commands;
using Flurl;
using Microsoft.Extensions.Logging;
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
        private readonly IOptions<ApiOptions> _api;
        private readonly IHttpClientFactory _factory;

        public ReactionRoleModule(
            IOptions<ApiOptions> api,
            IOptions<DiscordOptions> config,
            ILogger<ReactionRoleModule> logger,
            IHttpClientFactory factory) :
            base(config, logger)
        {
            _api = api;
            _factory = factory;
        }

        [Command("make")]
        [Alias("m", "setup")]
        [Remarks("reactionrole make [description]")]
        [Summary("Create a reaction role message.")]
        public async Task MakeAsync([Remainder] string description = "")
        {
            var embed = new EmbedBuilder()
                .WithTitle("React to receive roles")
                .WithDescription(description)
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
            // Get emote string representation and guild role by role id.
            string emoteText = emote.GetStringRepresentation();
            if (emoteText.Length > 100)
            {
                await ReplyAsync("Emote has too many characters.");
                return;
            }

            var guildRole = Context.Guild.GetRole(role.Id);

            var reactionRole = new ReactionRole
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                MessageId = message.Id,
                RoleId = guildRole.Id,
                Reaction = emoteText
            };

            // Send post request to api using json body.
            string json = JsonSerializer.Serialize(reactionRole);
            using var client = _factory.CreateClient();

            try
            {
                await client.PostAsync(_api.Value.ReactionRolesUri, new StringContent(json, Encoding.UTF8, "application/json"));
                await message.AddReactionAsync(emote);
                await ReplyAsync($"Added reaction role {guildRole.Mention}.", allowedMentions: AllowedMentions.None);
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
            // Get guild role by role id.
            var guildRole = Context.Guild.GetRole(role.Id);

            // Get reaction role that references this message and role.
            var reactionRole = await GetReactionRole(message, role);

            if (reactionRole is null)
            {
                await ReplyAsync("Could not find that reaction role.");
                return;
            }

            // Parse emote or emoji.
            IEmote reaction = Emote.TryParse(reactionRole.Reaction, out var emote)
                ? (IEmote) emote
                : new Emoji(reactionRole.Reaction);

            // Send delete request to api using id.
            string command = _api.Value.ReactionRolesUri.AppendPathSegment(reactionRole.Id.ToString());
            using var client = _factory.CreateClient();

            try
            {
                await client.DeleteAsync(command);
                await message.RemoveAllReactionsForEmoteAsync(reaction);
                await ReplyAsync($"Removed reaction role {guildRole.Mention}.", allowedMentions: AllowedMentions.None);
            }
            catch
            {
                await ReplyAsync("Could not remove reaction role.");
                throw;
            }
        }

        [Command("edit")]
        [Alias("e")]
        [Remarks("reactionrole edit <messageId> \"[title]\" [description]")]
        [Summary(
            "Edit a reaction role message with a new title and description. Titles with spaces must be wrapped in quotes.")]
        public async Task EditAsync(IMessage message, string title = "", [Remainder] string description = "")
        {
            // Must be a user message sent by the bot.
            if (message.Author.Id != Context.Client.CurrentUser.Id ||
                message is not IUserMessage original)
            {
                await ReplyAsync("That message is not mine to edit. *(੭*ˊᵕˋ)੭*ଘ");
                return;
            }

            // Get embed from message.
            var embed = original.Embeds.FirstOrDefault();
            if (embed is null)
            {
                await ReplyAsync("That message does not contain an embed. (╯°□°）╯︵ ┻━┻");
                return;
            }

            // Edit the embed with the new description.
            var edited = embed.ToEmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .Build();

            try
            {
                await original.ModifyAsync(properties => properties.Embed = edited);
            }
            catch
            {
                await ReplyAsync("Error editing reaction role message.");
            }
        }

        [Command("clear")]
        [Alias("c")]
        [Remarks("reactionrole clear <messageId>")]
        [Summary("Clears all reaction roles from a message.")]
        public async Task ClearAsync(IMessage message)
        {
            await message.RemoveAllReactionsAsync();
        }

        private async Task<ReactionRole?> GetReactionRole(IMessage message, IRole role)
        {
            // Get list of tags from this guild.
            string query = _api.Value.ReactionRolesUri
                .SetQueryParam("messageId", message.Id.ToString())
                .SetQueryParam("roleId", role.Id.ToString());

            // Send get request to api using query parameters for tag name and guild id.
            using var client = _factory.CreateClient();
            var response = await client.GetFromJsonAsync<IEnumerable<ReactionRole>>(query);
            var reactionRole = response?.FirstOrDefault();

            return reactionRole;
        }
    }
}
