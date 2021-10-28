namespace Discord;

public static class DiscordExtensions
{
    public static string GetStringRepresentation(this IEmote emote) =>
        emote switch
        {
            Emote e => e.ToString(),
            Emoji e => e.ToString(),
            _ => string.Empty
        };
}
