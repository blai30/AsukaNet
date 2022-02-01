using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Asuka.Models;

public record UrbanResponse
{
    [JsonPropertyName("list")]
    public List<List>? UrbanEntries { get; init; } = null;
}

public record List
{
    [JsonPropertyName("definition")]
    public string? Definition { get; init; } = null;

    [JsonPropertyName("permalink")]
    public string? Permalink { get; init; } = null;

    [JsonPropertyName("thumbs_up")]
    public int? ThumbsUp { get; init; } = null;

    [JsonPropertyName("thumbs_down")]
    public int? ThumbsDown { get; init; } = null;

    [JsonPropertyName("author")]
    public string? Author { get; init; } = null;

    [JsonPropertyName("word")]
    public string? Word { get; init; } = null;

    [JsonPropertyName("written_on")]
    public DateTime? WrittenOn { get; init; } = null;

    [JsonPropertyName("example")]
    public string? Example { get; init; } = null;
}
