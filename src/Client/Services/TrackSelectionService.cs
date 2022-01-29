using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace Asuka.Services;

public class TrackSelectionService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly IOptions<DiscordOptions> _config;
    private readonly LavaNode _lavaNode;
    private readonly ILogger<InteractionHandlerService> _logger;

    public TrackSelectionService(
        DiscordSocketClient client,
        IOptions<DiscordOptions> config,
        LavaNode lavaNode,
        ILogger<InteractionHandlerService> logger)
    {
        _client = client;
        _config = config;
        _lavaNode = lavaNode;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.SelectMenuExecuted += OnSelectMenuOptionSelected;

        _logger.LogInformation($"{GetType().Name} started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.SelectMenuExecuted -= OnSelectMenuOptionSelected;

        _logger.LogInformation($"{GetType().Name} stopped");
        await Task.CompletedTask;
    }

    private async Task OnSelectMenuOptionSelected(SocketMessageComponent component)
    {
        if (component.Message.Author.Id != _client.CurrentUser.Id) return;
        if (component.User is not SocketGuildUser user) return;
        if (!component.Data.CustomId.StartsWith("tracks:")) return;

        var player = _lavaNode.GetPlayer(user.Guild);
        string? selection = component.Data.Values.FirstOrDefault();

        var search = await _lavaNode.SearchAsync(SearchType.Direct, selection);
        var track = search.Tracks.First();

        // Player is already playing or paused but still has remaining tracks, enqueue new track.
        if (player.Track is not null &&
            player.PlayerState is PlayerState.Playing or PlayerState.Paused)
        {
            player.Queue.Enqueue(track);

            // Announce the track that was enqueued.
            string artwork = await track.FetchArtworkAsync();
            var embed = new EmbedBuilder()
                .WithTitle(track.Title)
                .WithUrl(track.Url)
                .WithAuthor($"Enqueued #{player.Queue.Count.ToString()}")
                .WithDescription(track.Duration.ToString("c"))
                .WithColor(_config.Value.EmbedColor)
                .WithThumbnailUrl(artwork)
                .Build();

            _logger.LogTrace($"Enqueued: {track.Title} in {user.Guild.Name}");
            await component.Message.ModifyAsync(properties =>
            {
                properties.Content = null;
                properties.Components = null;
                properties.Embed = embed;
            });
        }
        else
        {
            await player.PlayAsync(track);
            await component.Message.DeleteAsync();
        }
    }
}
