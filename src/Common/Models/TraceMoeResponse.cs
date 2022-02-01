using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Asuka.Models.Api.TraceMoe;

public record TraceMoeResponse
{
    [JsonPropertyName("result")]
    public IEnumerable<Result>? Result { get; init; }
}

public record Result
{
    [JsonPropertyName("from")]
    public float From { get; init; }

    [JsonPropertyName("to")]
    public float To { get; init; }

    [JsonPropertyName("episode")]
    public object? Episode { get; init; }

    [JsonPropertyName("similarity")]
    public float Similarity { get; init; }

    [JsonPropertyName("anilist")]
    public Anilist? Anilist { get; init; } = null;

    [JsonPropertyName("filename")]
    public string? Filename { get; init; } = null;
}

public record Anilist
{
    [JsonPropertyName("id")]
    public ulong Id { get; init; }

    [JsonPropertyName("idMal")]
    public ulong IdMal { get; init; }

    [JsonPropertyName("title")]
    public Title? Title { get; init; } = null;

    [JsonPropertyName("synonyms")]
    public string[]? Synonyms { get; init; } = null;

    [JsonPropertyName("isAdult")]
    public bool IsAdult { get; init; }
}

public record Title
{
    [JsonPropertyName("native")]
    public string? Native { get; init; } = null;

    [JsonPropertyName("romaji")]
    public string? Romaji { get; init; } = null;

    [JsonPropertyName("english")]
    public string? English { get; init; } = null;
}
