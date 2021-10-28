using System.Text.Json.Serialization;

namespace Asuka.Models.Api.Asuka;

public record ReactionRole
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; init; }
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; init; }
    [JsonPropertyName("message_id")]
    public ulong MessageId { get; init; }
    [JsonPropertyName("role_id")]
    public ulong RoleId { get; init; }
    [JsonPropertyName("reaction")]
    public string Reaction { get; init; }
}
