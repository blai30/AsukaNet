using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Roles;

[Group("roleassigner")]
[Alias("ra")]
[Remarks("Roles")]
[Summary("Manage self-service roles assignment for the server.")]
[RequireBotPermission(
    ChannelPermission.ManageMessages |
    ChannelPermission.ManageRoles |
    ChannelPermission.ReadMessageHistory |
    ChannelPermission.ViewChannel)]
[RequireUserPermission(
    ChannelPermission.ManageMessages |
    ChannelPermission.ManageRoles |
    ChannelPermission.ReadMessageHistory |
    ChannelPermission.ViewChannel)]
[RequireContext(ContextType.Guild)]
public class RoleAssignerModule : CommandModuleBase
{
    public RoleAssignerModule(
        IOptions<DiscordOptions> config,
        ILogger<RoleAssignerModule> logger) :
        base(config, logger)
    {
    }

    [Command("make")]
    [Alias("m", "setup")]
    [Remarks("roleassigner make")]
    [Summary("Create a role assigner message.")]
    public async Task MakeAsync([Remainder] string? title = null)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title ?? "Assign roles for yourself")
            .WithColor(Config.Value.EmbedColor)
            .Build();

        var components = new ComponentBuilder().Build();

        await ReplyAsync(embed: embed, components: components);
    }

    [Command("add")]
    [Alias("a")]
    [Remarks("roleassigner add <messageId> <:emoji:> <@role>")]
    [Summary("Add a role assignment to a role assigner message.")]
    public async Task AddAsync(SocketUserMessage message, IRole role, IEmote? emote = null)
    {
        var componentBuilder = ComponentBuilder.FromComponents(message.Components);
        componentBuilder.WithButton(
            $"@{role.Name}",
            emote: emote,
            customId: $"roleassigner-{message.Id}-{role.Id}",
            style: ButtonStyle.Secondary);

        await message.ModifyAsync(properties => properties.Components = componentBuilder.Build());
    }

    [Command("remove")]
    [Alias("r")]
    [Remarks("roleassigner remove <messageId> <@role>")]
    [Summary("Remove a reaction role from a reaction role message.")]
    public async Task RemoveAsync(SocketUserMessage message, IRole role)
    {
        var componentBuilder = new ComponentBuilder();

        // Rebuild components without the role.
        IList<ActionRowBuilder> rows = message.Components
            .Select(row => row.Components
                .Where(component => component.CustomId != $"{message.Id}-{role.Id}").ToList())
            .Where(components => components.Count > 0)
            .Select(components => new ActionRowBuilder().WithComponents(components)).ToList();

        componentBuilder.WithRows(rows);

        await message.ModifyAsync(properties => properties.Components = componentBuilder.Build());
    }

    [Command("edit")]
    [Alias("e")]
    [Remarks("roleassigner edit <messageId> \"[title]\"")]
    [Summary("Edit a role assigner message with a new title.")]
    public async Task EditAsync(SocketUserMessage message, [Remainder] string title)
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

        // Edit the embed with the new title.
        var edited = embed.ToEmbedBuilder()
            .WithTitle(title)
            .Build();

        try
        {
            await original.ModifyAsync(properties => properties.Embed = edited);
        }
        catch
        {
            await ReplyAsync("Error editing message.");
        }
    }

    [Command("clear")]
    [Alias("c")]
    [Remarks("roleassigner clear <messageId>")]
    [Summary("Clears all reaction roles from a message.")]
    public async Task ClearAsync(SocketUserMessage message)
    {
        var componentBuilder = new ComponentBuilder();
        await message.ModifyAsync(properties => properties.Components = componentBuilder.Build());
    }
}
