using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Victoria;
using Victoria.Enums;

namespace Asuka.Modules.Music
{
    [Group("music")]
    [Alias("m")]
    [Remarks("Music")]
    [Summary("Play music in a voice channel.")]
    [RequireContext(ContextType.Guild)]
    public sealed class MusicModule : CommandModuleBase
    {
        private readonly LavaNode _lavaNode;

        public MusicModule(
            IOptions<DiscordOptions> config,
            ILogger<CommandModuleBase> logger,
            LavaNode lavaNode) :
            base(config, logger)
        {
            _lavaNode = lavaNode;
        }

        [Command("join")]
        [Alias("j")]
        [Remarks("music join")]
        [Summary("Join a voice channel in the server.")]
        public async Task JoinAsync()
        {
            // TODO: Join if playing but kicked from voice channel.
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already playing in a voice channel in this server.");
                return;
            }

            await TryJoinAsync();
        }

        public async Task LeaveAsync()
        {
        }

        [Command("play")]
        [Alias("p")]
        [Remarks("music play <query>")]
        [Summary("Play music in a voice channel.")]
        public async Task PlayAsync([Remainder] string searchQuery = "")
        {
            // Join if not playing.
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await TryJoinAsync();
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You must be in the same voice channel.");
                return;
            }

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide a query.");
                return;
            }

            try
            {
                // Play from url or search YouTube.
                var search = Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute)
                    ? await _lavaNode.SearchAsync(searchQuery)
                    : await _lavaNode.SearchYouTubeAsync(searchQuery);

                if (search.LoadStatus is LoadStatus.NoMatches || search.LoadStatus is LoadStatus.LoadFailed)
                {
                    await ReplyAsync($"No matches found for query `{searchQuery.Truncate(100, "...")}`");
                    return;
                }

                // TODO: Interactive select from list.
                var track = search.Tracks[0];

                // Player is already playing or paused but still has remaining tracks, enqueue new track.
                if (player.Track != null &&
                    player.PlayerState is PlayerState.Playing ||
                    player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);

                    Logger.LogTrace($"Enqueued: {track.Title} in {Context.Guild.Name}");
                    await ReplyAsync($"Enqueued: `{track.Title}` at position `{player.Queue.Count}`.");
                }
                else
                {
                    await player.PlayAsync(track);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                await ReplyAsync(e.Message);
            }
        }

        public async Task PauseAsync()
        {
        }

        public async Task QueueAsync()
        {
        }

        public async Task SkipAsync()
        {
        }

        public async Task ClearAsync()
        {
        }

        private async Task TryJoinAsync()
        {
            if (Context.User is not IVoiceState user ||
                user.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel.");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(user.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {user.VoiceChannel.Name}!");
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                await ReplyAsync(e.Message);
            }
        }

        private async Task EnqueueAsync()
        {
        }

        private async Task DequeueAsync()
        {
        }
    }
}
