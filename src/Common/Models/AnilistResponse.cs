using System;
using System.Text.Json.Serialization;

namespace Asuka.Models;

public record AnilistResponse
{
    [JsonPropertyName("Media")]
    public Media? Media { get; init; } = null;
}

public record Media
{
    [JsonPropertyName("coverImage")]
    public CoverImage? CoverImage { get; init; } = null;
}

public record CoverImage
{
    [JsonPropertyName("large")]
    public Uri? Large { get; init; } = null;
}
