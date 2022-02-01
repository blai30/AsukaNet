using System;
using System.Text.Json.Serialization;

namespace Asuka.Models;

public record Tag
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("reaction")]
    public string? Reaction { get; init; }

    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; init; }

    [JsonPropertyName("user_id")]
    public ulong UserId { get; init; }

    [JsonPropertyName("usage_count")]
    public int UsageCount { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; init; }
}
