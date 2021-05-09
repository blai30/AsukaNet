using System.Text.Json.Serialization;

namespace Asuka.Models.Api.Asuka
{
    public class ReactionRole
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("guild_id")]
        public ulong GuildId { get; set; }
        [JsonPropertyName("channel_id")]
        public ulong ChannelId { get; set; }
        [JsonPropertyName("message_id")]
        public ulong MessageId { get; set; }
        [JsonPropertyName("role_id")]
        public ulong RoleId { get; set; }
        [JsonPropertyName("reaction")]
        public string Reaction { get; set; }
    }
}
