using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victoria;

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

            _logger.LogInformation($"{GetType().Name} started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Ready -= OnReadyAsync;

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
    }
}
