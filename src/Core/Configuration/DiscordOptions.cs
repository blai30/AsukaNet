using Discord;

namespace Asuka.Configuration;

public record DiscordOptions
{
    public string BotPrefix { get; init; }
    public uint EmbedColor { get; init; }
    public string InviteUrl { get; init; }
    public string GitHubUrl { get; init; }
    public ulong OwnerId { get; init; }
    public Status Status { get; init; }
}

/// <summary>
///     Status object from appsettings.json.
///     Name of class/struct and properties must match.
/// </summary>
public struct Status
{
    public ActivityType Activity { get; init; }
    public string Game { get; init; }
}
