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
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel in this server.");
                return;
            }

            if (Context.User is not IVoiceState voiceState ||
                voiceState.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel.");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }

        [Command("play")]
        [Alias("p")]
        [Remarks("music play <query>")]
        [Summary("Play music in a voice channel.")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            string[] queries = searchQuery.Split(' ');
            foreach (string query in queries)
            {
                var searchResponse = await _lavaNode.SearchAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await ReplyAsync($"I wasn't able to find anything for `{query.Truncate(50, "...")}`.");
                    return;
                }

                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks[0];
                        player.Queue.Enqueue(track);
                        await ReplyAsync($"Enqueued: {track.Title}");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks[0];

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (int i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                await ReplyAsync($"Now Playing: {track.Title}");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks[i]);
                            }
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await ReplyAsync($"Now Playing: {track.Title}");
                    }
                }
            }
        }
    }
}
