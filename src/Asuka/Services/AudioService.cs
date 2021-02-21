using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.EventArgs;

namespace Asuka.Services
{
    public class AudioService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly LavaNode _lavaNode;
        private readonly ILogger<AudioService> _logger;

        public AudioService(
            DiscordSocketClient client,
            LavaNode lavaNode,
            ILogger<AudioService> logger)
        {
            _client = client;
            _lavaNode = lavaNode;
            _logger = logger;
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
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
                _logger.LogInformation("Lava Node connected");
            }
        }

        private async Task OnTrackStarted(TrackStartEventArgs args)
        {
            var player = args.Player;
            _logger.LogTrace($"Playing: {args.Track.Title} in {args.Player.VoiceChannel.Guild.Name}");
            await player.TextChannel.SendMessageAsync($"Playing: `{player.Track.Title}`.");
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext()) {
                return;
            }

            var player = args.Player;
            await player.TextChannel.SendMessageAsync($"{args.Reason}: `{args.Track.Title}`.");
            await player.TextChannel.SendMessageAsync($"`{player.Queue.Count}` tracks left in the queue.");

            // Check next track in the queue or leave voice channel if queue is empty.
            if (!player.Queue.TryDequeue(out var track)) {
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                return;
            }

            // Play next track in the queue.
            await player.PlayAsync(track);
        }
    }
}
