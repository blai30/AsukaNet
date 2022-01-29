using System;
using System.Linq;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace Asuka.Modules;

[Group(
    "music",
    "Play music in a voice channel.")]
[RequireContext(ContextType.Guild)]
public sealed class MusicModule : InteractionModule
{
    private readonly LavaNode _lavaNode;

    public MusicModule(
        IOptions<DiscordOptions> config,
        ILogger<MusicModule> logger,
        LavaNode lavaNode) :
        base(config, logger)
    {
        _lavaNode = lavaNode;
    }

    [SlashCommand(
        "join",
        "Join a voice channel in the server.")]
    public async Task JoinAsync()
    {
        // TODO: Music join if playing but kicked from voice channel.
        if (_lavaNode.HasPlayer(Context.Guild))
        {
            await RespondAsync("I'm already playing in a voice channel in this server.");
            return;
        }

        await TryJoinAsync(true);
    }

    [SlashCommand(
        "leave",
        "Leave the currently playing voice channel.")]
    public async Task LeaveAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await RespondAsync("Currently not playing.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
        {
            await RespondAsync("You must be in the same voice channel.");
            return;
        }

        try
        {
            // Stop playing and leave.
            if (player.PlayerState is PlayerState.Playing)
            {
                await player.StopAsync();
            }

            var channel = player.VoiceChannel;
            await _lavaNode.LeaveAsync(channel);

            Logger.LogTrace($"Left: {channel.Name} in {Context.Guild}");

            int bitrateKb = channel.Bitrate / 1000;
            var embed = new EmbedBuilder()
                .WithTitle(channel.Name)
                .WithAuthor("Left")
                .WithDescription($"{bitrateKb.ToString()} kbps")
                .WithColor(Config.Value.EmbedColor)
                .Build();

            await ReplyAsync(embed: embed);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogError(e.ToString());
            await RespondAsync(e.Message);
        }
    }

    [SlashCommand(
        "play",
        "Play music in a voice channel.")]
    public async Task PlayAsync(string searchQuery)
    {
        // Join if not playing.
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await TryJoinAsync();
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
        {
            await RespondAsync("You must be in the same voice channel.");
            return;
        }

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await RespondAsync("Please provide a query.");
            return;
        }

        await DeferAsync();

        try
        {
            // Play from url or search YouTube.
            var search = Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute)
                ? await _lavaNode.SearchAsync(SearchType.Direct, searchQuery)
                : await _lavaNode.SearchYouTubeAsync(searchQuery);

            if (search.Status is SearchStatus.NoMatches or SearchStatus.LoadFailed)
            {
                await RespondAsync($"No matches found for query `{searchQuery.Truncate(100, "...")}`");
                return;
            }

            // TODO: Music enqueue interactive select from list.
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
                    .WithColor(Config.Value.EmbedColor)
                    .WithThumbnailUrl(artwork)
                    .Build();

                Logger.LogTrace($"Enqueued: {track.Title} in {Context.Guild.Name}");
                await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Embed = embed);
            }
            else
            {
                await player.PlayAsync(track);
                await DeleteOriginalResponseAsync();
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            await RespondAsync(e.Message);
        }
    }

    [SlashCommand(
        "pause",
        "Pause the currently playing track or resume playing.")]
    public async Task PauseAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await RespondAsync("Currently not playing.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
        {
            await RespondAsync("You must be in the same voice channel.");
            return;
        }

        var state = player.PlayerState;
        if (state is not PlayerState.Playing and not PlayerState.Paused)
        {
            return;
        }

        var track = player.Track;
        string status = string.Empty;

        try
        {
            // Pause currently playing track if player is playing or resume player if paused.
            if (state is PlayerState.Playing)
            {
                await player.PauseAsync();
                status = "Pausing";
            }

            if (state is PlayerState.Paused)
            {
                await player.ResumeAsync();
                status = "Resuming";
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            await RespondAsync(e.Message);
        }

        string position = track.Position.ToString("hh\\:mm\\:ss");
        string duration = track.Duration.ToString("hh\\:mm\\:ss");

        string artwork = await track.FetchArtworkAsync();
        var embed = new EmbedBuilder()
            .WithTitle(track.Title)
            .WithUrl(track.Url)
            .WithAuthor(status)
            .WithDescription($"{position} / {duration}")
            .WithColor(Config.Value.EmbedColor)
            .WithThumbnailUrl(artwork)
            .Build();

        Logger.LogTrace($"{status}: {track.Title} in {Context.Guild}");
        await RespondAsync(embed: embed);
    }

    [SlashCommand(
        "skip",
        "Skips the currently playing track.")]
    public async Task SkipAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await RespondAsync("Currently not playing.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
        {
            await RespondAsync("You must be in the same voice channel.");
            return;
        }

        if (player.Track is null)
        {
            await RespondAsync("Nothing to skip.");
            return;
        }

        try
        {
            // Skip current track if there are more in the queue or stop track if only one.
            var currentTrack = player.Track;
            int queueCount = player.Queue.Count;
            if (queueCount >= 1)
            {
                await player.SkipAsync();
            }
            else if (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
            {
                await player.StopAsync();
            }

            // Announce skipped track.
            string artwork = await currentTrack.FetchArtworkAsync();
            var embed = new EmbedBuilder()
                .WithTitle(currentTrack.Title)
                .WithUrl(currentTrack.Url)
                .WithAuthor("Skipping")
                .WithDescription($"`{queueCount.ToString()}` track(s) left in the queue.")
                .WithColor(Config.Value.EmbedColor)
                .WithThumbnailUrl(artwork)
                .Build();

            Logger.LogTrace($"Skipped: {currentTrack.Title} in {Context.Guild}");
            await RespondAsync(embed: embed);
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            await RespondAsync(e.Message);
        }
    }

    [SlashCommand(
        "remove",
        "Removes a track from the queue by index.")]
    public async Task RemoveAsync(int index)
    {
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await RespondAsync("Currently not playing.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
        {
            await RespondAsync("You must be in the same voice channel.");
            return;
        }

        try
        {
            var track = player.Queue.RemoveAt(index - 1);

            // Announce the track that was removed.
            string artwork = await track.FetchArtworkAsync();
            var embed = new EmbedBuilder()
                .WithTitle(track.Title)
                .WithUrl(track.Url)
                .WithAuthor($"Removed #{index.ToString()}")
                .WithDescription(track.Duration.ToString("c"))
                .WithColor(Config.Value.EmbedColor)
                .WithThumbnailUrl(artwork)
                .Build();

            Logger.LogTrace($"Removed: {track.Title} in {Context.Guild.Name}");
            await RespondAsync(embed: embed);
        }
        catch (Exception)
        {
            Logger.LogTrace($"Failed removing track at index {index.ToString()} in {Context.Guild.Name}");
            await RespondAsync($"No track in queue at index {index.ToString()}.");
        }
    }

    [SlashCommand(
        "queue",
        "View the current music queue.")]
    public async Task QueueAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await RespondAsync("Currently not playing.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (player.Queue.Count <= 0)
        {
            await RespondAsync("Nothing in the queue.");
            return;
        }

        // TODO: Music queue interactive pagination.
        var tracks = player.Queue
            .Take(10)
            .Select((track, index) => $"{(index + 1).ToString()}. `{track.Title}`")
            .ToList();
        string list = string.Join("\n", tracks);

        var embed = new EmbedBuilder()
            .WithTitle($"{player.Queue.Count.ToString()} tracks left")
            .WithAuthor("Queue")
            .WithDescription(list.Truncate(2048, "..."))
            .WithColor(Config.Value.EmbedColor)
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand(
        "clear",
        "Clears the music queue.")]
    public async Task ClearAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild) is false)
        {
            await RespondAsync("Currently not playing.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (Context.User is IVoiceState user && user.VoiceChannel != player.VoiceChannel)
        {
            await RespondAsync("You must be in the same voice channel.");
            return;
        }

        if (player.Queue.Count <= 0)
        {
            await RespondAsync("Nothing in the queue to clear.");
            return;
        }

        try
        {
            int count = player.Queue.Count;
            player.Queue.Clear();

            var embed = new EmbedBuilder()
                .WithAuthor("Cleared")
                .WithDescription($"`{count.ToString()}` track(s) removed from the queue.")
                .WithColor(Config.Value.EmbedColor)
                .Build();

            await RespondAsync(embed: embed);
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            await RespondAsync(e.Message);
        }
    }

    private async Task TryJoinAsync(bool respond = false)
    {
        if (Context.User is not IVoiceState user ||
            user.VoiceChannel is null)
        {
            await RespondAsync("You must be connected to a voice channel.");
            return;
        }

        try
        {
            await _lavaNode.JoinAsync(user.VoiceChannel, Context.Channel as ITextChannel);

            int bitrateKb = user.VoiceChannel.Bitrate / 1000;
            var embed = new EmbedBuilder()
                .WithTitle(user.VoiceChannel.Name)
                .WithAuthor("Joined")
                .WithDescription($"{bitrateKb.ToString()} kbps")
                .WithColor(Config.Value.EmbedColor)
                .Build();

            Logger.LogTrace($"Joined: {user.VoiceChannel.Name} in {Context.Guild}");
            if (respond)
            {
                await RespondAsync(embed: embed);
            }
            else
            {
                await ReplyAsync(embed: embed);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            await RespondAsync(e.Message);
        }
    }
}
