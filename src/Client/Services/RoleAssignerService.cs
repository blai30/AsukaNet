using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Services;

public class RoleAssignerService : IHostedService
{
    private readonly IOptions<DiscordOptions> _config;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<RoleAssignerService> _logger;

    public RoleAssignerService(
        IOptions<DiscordOptions> config,
        DiscordSocketClient client,
        ILogger<RoleAssignerService> logger)
    {
        _config = config;
        _client = client;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.ButtonExecuted += OnButtonClicked;

        _logger.LogInformation($"{GetType().Name} started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.ButtonExecuted -= OnButtonClicked;

        _logger.LogInformation($"{GetType().Name} stopped");
        await Task.CompletedTask;
    }

    private async Task OnButtonClicked(SocketMessageComponent component)
    {
        if (component.Message.Author.Id != _client.CurrentUser.Id) return;
        if (component.User is not SocketGuildUser user) return;

        string[] tokens = component.Data.CustomId.Split('-');
        if (tokens[0] != "roleassigner") return;

        var role = user.Guild.GetRole(Convert.ToUInt64(tokens[2]));
        if (role is null) return;

        if (user.Roles.Any(r => r.Id == role.Id))
        {
            await user.RemoveRoleAsync(role);

            var embed = new EmbedBuilder()
                .WithTitle("Removed role")
                .WithAuthor(user)
                .WithDescription(role.Mention)
                .WithColor(_config.Value.EmbedColor)
                .Build();

            await component.RespondAsync(embed: embed, ephemeral: true);
            _logger.LogTrace($"{role.Mention} is removed from {user.Mention}");
        }
        else
        {
            await user.AddRoleAsync(role);

            var embed = new EmbedBuilder()
                .WithTitle("Granted role")
                .WithAuthor(user)
                .WithDescription(role.Mention)
                .WithColor(_config.Value.EmbedColor)
                .Build();

            await component.RespondAsync(embed: embed, ephemeral: true);
            _logger.LogTrace($"{role.Mention} is granted to {user.Mention}");
        }
    }
}
