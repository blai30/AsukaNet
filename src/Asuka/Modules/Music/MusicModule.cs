using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel in this server.");
                return;
            }

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

        [Command("play")]
        [Alias("p")]
        [Remarks("music play <query>")]
        [Summary("Play music in a voice channel.")]
        public async Task PlayAsync([Remainder] string searchQuery = "")
        {
            if (Context.User is not IVoiceState user ||
                user.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel.");
                return;
            }

            bool connected = _lavaNode.TryGetPlayer(Context.Guild, out var player);
            if (connected && user.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You must be in the same voice channel.");
                return;
            }

            if (!connected)
            {
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

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide a query.");
                return;
            }

            try
            {
                // var search = Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute)
                //     ? await _lavaNode.SearchAsync(searchQuery)
                //     : await _lavaNode.SearchYouTubeAsync(searchQuery);
                var search = await _lavaNode.SearchYouTubeAsync(searchQuery);

                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    await ReplyAsync($"No matches found for query `{searchQuery.Truncate(100, "...")}`");
                    return;
                }

                // TODO: Interactive select from list.
                var track = search.Tracks[0];

                // Bot is already playing music or paused but still has remaining tracks, enqueue new track.
                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    Logger.LogTrace($"Enqueued {track.Title} in {Context.Guild.Name}");
                    await ReplyAsync($"Enqueued `{track.Title}`.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                await ReplyAsync(e.Message);
            }
        }
    }
}
