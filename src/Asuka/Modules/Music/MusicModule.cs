using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Services;
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

        [Command("leave")]
        [Alias("l")]
        [Remarks("music leave")]
        [Summary("Leave the currently playing voice channel.")]
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

                    // Announce the track that was enqueued.
                    var embed = await AudioService.BuildTrackEmbed("Enqueued", track, Config.Value.EmbedColor);

                    Logger.LogTrace($"Enqueued: {track.Title} in {Context.Guild.Name}");
                    await ReplyAsync(embed: embed);
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

        [Command("pause")]
        [Remarks("music pause")]
        [Summary("Pause the currently playing track.")]
        public async Task PauseAsync()
        {
        }

        [Command("queue")]
        [Alias("q")]
        [Remarks("music queue")]
        [Summary("View the current music queue.")]
        public async Task QueueAsync()
        {
        }

        [Command("skip")]
        [Alias("s")]
        [Remarks("music skip")]
        [Summary("Skips the currently playing track.")]
        public async Task SkipAsync()
        {
        }

        [Command("clear")]
        [Alias("c")]
        [Remarks("music clear")]
        [Summary("Clears the music queue.")]
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

                var embed = new EmbedBuilder()
                    .WithTitle(user.VoiceChannel.Name)
                    .WithAuthor("Joined")
                    .WithDescription($"{user.VoiceChannel.Bitrate / 1000} kbps")
                    .WithColor(Config.Value.EmbedColor)
                    .Build();

                await ReplyAsync(embed: embed);
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
