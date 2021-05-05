using System;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Victoria;
using Victoria.EventArgs;

namespace Asuka.Services
{
    public class AudioService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptions<DiscordOptions> _config;
        private readonly LavaNode _lavaNode;
        private readonly ILogger<AudioService> _logger;

        public AudioService(
            DiscordSocketClient client,
            IOptions<DiscordOptions> config,
            LavaNode lavaNode,
            ILogger<AudioService> logger)
        {
            _client = client;
            _lavaNode = lavaNode;
            _logger = logger;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReadyAsync;
            _lavaNode.OnTrackStarted += OnTrackStarted;
            _lavaNode.OnTrackEnded += OnTrackEnded;

            _logger.LogInformation($"{GetType().Name} started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Ready -= OnReadyAsync;
            _lavaNode.OnTrackStarted -= OnTrackStarted;
            _lavaNode.OnTrackEnded -= OnTrackEnded;

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

        private async Task OnTrackStarted(TrackStartEventArgs args)
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

            _logger.LogTrace($"Playing: {args.Track.Title} in {args.Player.VoiceChannel.Guild.Name}");
            await player.TextChannel.SendMessageAsync(embed: embed);
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason.ShouldPlayNext() is false)
            {
                return;
            }

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
                await Task.Delay(TimeSpan.FromSeconds(60))
                    .ContinueWith(_ => _lavaNode.LeaveAsync(player.VoiceChannel));
                return;
            }

            // Play next track in the queue.
            await player.PlayAsync(nextTrack);
        }
    }
}
