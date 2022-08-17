using System;
using System.Collections.Concurrent;
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
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace Asuka.Services;

public class AudioService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly IOptions<DiscordOptions> _config;
    private readonly LavaNode _lavaNode;
    private readonly ILogger<AudioService> _logger;
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens = new();

    public AudioService(
        DiscordSocketClient client,
        IOptions<DiscordOptions> config,
        LavaNode lavaNode,
        ILogger<AudioService> logger)
    {
        _client = client;
        _config = config;
        _lavaNode = lavaNode;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += OnReadyAsync;
        _client.UserVoiceStateUpdated += OnVoiceStateUpdatedAsync;
        _client.SelectMenuExecuted += OnSelectMenuOptionSelectedAsync;
        _lavaNode.OnTrackStarted += OnTrackStartedAsync;
        _lavaNode.OnTrackEnded += OnTrackEndedAsync;

        _logger.LogInformation($"{GetType().Name} started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Ready -= OnReadyAsync;
        _client.UserVoiceStateUpdated -= OnVoiceStateUpdatedAsync;
        _client.SelectMenuExecuted -= OnSelectMenuOptionSelectedAsync;
        _lavaNode.OnTrackStarted -= OnTrackStartedAsync;
        _lavaNode.OnTrackEnded -= OnTrackEndedAsync;

        _logger.LogInformation($"{GetType().Name} stopped");
        await Task.CompletedTask;
    }

    private async Task OnReadyAsync()
    {
        if (_lavaNode.IsConnected is false)
        {
            await _lavaNode.ConnectAsync();
            _logger.LogInformation("Lava Node connected");
        }
    }

    private async Task OnVoiceStateUpdatedAsync(
        SocketUser user,
        SocketVoiceState before,
        SocketVoiceState after)
    {
        if (user.Id != _client.CurrentUser.Id) return;
        if (before.VoiceChannel is null || after.VoiceChannel is not null) return;

        await _lavaNode.LeaveAsync(before.VoiceChannel);
    }

    private async Task OnSelectMenuOptionSelectedAsync(SocketMessageComponent component)
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
            await component.UpdateAsync(properties =>
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

    private async Task OnTrackStartedAsync(TrackStartEventArgs args)
    {
        var player = args.Player;
        var track = args.Track;

        // Announce the track that will play.
        string artwork = await track.FetchArtworkAsync();
        var embed = new EmbedBuilder()
            .WithTitle(track.Title)
            .WithUrl(track.Url)
            .WithAuthor("Playing")
            .WithDescription(track.Duration.ToString("c"))
            .WithColor(_config.Value.EmbedColor)
            .WithThumbnailUrl(artwork)
            .Build();

        _logger.LogTrace($"Playing: {track.Title} in {player.VoiceChannel.Guild.Name}");
        await player.TextChannel.SendMessageAsync(embed: embed);

        // Handle auto disconnect when no more tracks are queued or a user leaves.
        if (_disconnectTokens.TryGetValue(args.Player.VoiceChannel.Id, out var tokenSource) is false)
        {
            return;
        }

        if (tokenSource.IsCancellationRequested)
        {
            return;
        }

        // Auto disconnect cancelled.
        tokenSource.Cancel(true);
    }

    private async Task OnTrackEndedAsync(TrackEndedEventArgs args)
    {
        var player = args.Player;
        var track = args.Track;

        // Announce the track that ended and how many more tracks in the queue.
        string artwork = await track.FetchArtworkAsync();
        var embed = new EmbedBuilder()
            .WithTitle(track.Title)
            .WithUrl(track.Url)
            .WithAuthor("Finished")
            .WithDescription($"`{player.Queue.Count.ToString()}` track(s) left in the queue.")
            .WithColor(_config.Value.EmbedColor)
            .WithThumbnailUrl(artwork)
            .Build();

        _logger.LogTrace($"Finished: {args.Track.Title} in {args.Player.VoiceChannel.Guild.Name}");
        await player.TextChannel.SendMessageAsync(embed: embed);

        // Check next track in the queue or leave voice channel if queue is empty.
        if (player.Queue.TryDequeue(out var nextTrack) is false)
        {
            _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(10));
            return;
        }

        // Play next track in the queue.
        await player.PlayAsync(nextTrack);
    }

    private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeout)
    {
        if (_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var tokenSource) is false)
        {
            tokenSource = new CancellationTokenSource();
            _disconnectTokens.TryAdd(player.VoiceChannel.Id, tokenSource);
        }
        else if (tokenSource.IsCancellationRequested)
        {
            _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), tokenSource);
            tokenSource = _disconnectTokens[player.VoiceChannel.Id];
        }

        if (SpinWait.SpinUntil(() => tokenSource.IsCancellationRequested, timeout))
        {
            return;
        }

        _logger.LogTrace($"Disconnecting from {player.VoiceChannel.Guild.Name}");
        await _lavaNode.LeaveAsync(player.VoiceChannel);
    }
}
