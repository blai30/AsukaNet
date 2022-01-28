using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules;

[Group(
    "roleassigner",
    "Manage self-service roles assignment for the server.")]
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
public class RoleAssignerModule : InteractionModule
{
    private const string Prefix = "RA";

    public RoleAssignerModule(
        IOptions<DiscordOptions> config,
        ILogger<RoleAssignerModule> logger) :
        base(config, logger)
    {
    }

    [SlashCommand(
        "make",
        "Create a role assigner message.")]
    public async Task MakeAsync(string? title = null)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title ?? "Assign roles for yourself")
            .WithColor(Config.Value.EmbedColor)
            .Build();

        var components = new ComponentBuilder().Build();
        await RespondAsync(embed: embed, components: components);
    }

    [SlashCommand(
        "add",
        "Add a role assignment to a role assigner message.")]
    public async Task AddAsync(SocketUserMessage message, IRole role, IEmote? emote = null)
    {
        var componentBuilder = ComponentBuilder.FromComponents(message.Components);
        componentBuilder.WithButton(
            $"@{role.Name}",
            emote: emote,
            customId: $"{Prefix}-{message.Id}-{role.Id}",
            style: ButtonStyle.Secondary);

        await message.ModifyAsync(properties => properties.Components = componentBuilder.Build());
    }

    [SlashCommand(
        "remove",
        "Remove a reaction role from a reaction role message.")]
    public async Task RemoveAsync(SocketUserMessage message, IRole role)
    {
        var componentBuilder = new ComponentBuilder();

        // Rebuild components without the role.
        IList<ActionRowBuilder> rows = message.Components
            .Select(row => row.Components
                .Where(component =>
                    component.CustomId != $"{Prefix}-{message.Id}-{role.Id}").ToList())
            .Where(components =>
                components.Count > 0)
            .Select(components =>
                new ActionRowBuilder().WithComponents(components))
            .ToList();

        componentBuilder.WithRows(rows);
        await message.ModifyAsync(properties => properties.Components = componentBuilder.Build());
    }

    [SlashCommand(
        "edit",
        "Edit a role assigner message with a new title.")]
    public async Task EditAsync(SocketUserMessage message, string title)
    {
        // Must be a user message sent by the bot.
        if (message.Author.Id != Context.Client.CurrentUser.Id)
        {
            await RespondAsync("That message is not mine to edit. *(੭*ˊᵕˋ)੭*ଘ");
            return;
        }

        // Get embed from message.
        var embed = message.Embeds.FirstOrDefault();
        if (embed is null)
        {
            await RespondAsync("That message does not contain an embed. (╯°□°）╯︵ ┻━┻");
            return;
        }

        try
        {
            await message.ModifyAsync(properties =>
                properties.Embed = embed.ToEmbedBuilder().WithTitle(title).Build());
        }
        catch
        {
            await RespondAsync("Error editing message.");
        }
    }

    [SlashCommand(
        "clear",
        "Clears all reaction roles from a message.")]
    public async Task ClearAsync(SocketUserMessage message)
    {
        var componentBuilder = new ComponentBuilder();
        await message.ModifyAsync(properties => properties.Components = componentBuilder.Build());
    }
}
