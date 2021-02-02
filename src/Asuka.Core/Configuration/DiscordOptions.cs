using Discord;

namespace Asuka.Configuration
{
    public class DiscordOptions
    {
        public string BotPrefix { get; set; }
        public uint EmbedColor { get; set; }
        public string InviteUrl { get; set; }
        public ulong OwnerId { get; set; }
        public Status Status { get; set; }
    }

    /// <summary>
    /// Status object from appsettings.json.
    /// Name of class/struct and properties must match.
    /// </summary>
    public struct Status
    {
        public ActivityType Activity { get; set; }
        public string Game { get; set; }
    }
}
