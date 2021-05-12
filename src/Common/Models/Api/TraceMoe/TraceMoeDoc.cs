using System.Text.Json.Serialization;

namespace Asuka.Models.Api.TraceMoe
{
    public record TraceMoeDoc
    {
        [JsonPropertyName("from")]
        public float From { get; init; }

        [JsonPropertyName("to")]
        public float To { get; init; }

        [JsonPropertyName("at")]
        public float At { get; init; }

        [JsonPropertyName("episode")]
        public object? Episode { get; init; }

        [JsonPropertyName("similarity")]
        public float Similarity { get; init; }

        [JsonPropertyName("anilist_id")]
        public ulong AnilistId { get; init; }

        [JsonPropertyName("mal_id")]
        public ulong? MalId { get; init; }

        [JsonPropertyName("is_adult")]
        public bool IsAdult { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("title_native")]
        public string? TitleNative { get; init; }

        [JsonPropertyName("title_chinese")]
        public string? TitleChinese { get; init; }

        [JsonPropertyName("title_english")]
        public string? TitleEnglish { get; init; }

        [JsonPropertyName("title_romaji")]
        public string? TitleRomaji { get; init; }

        [JsonPropertyName("synonyms")]
        public string[]? Synonyms { get; init; }

        [JsonPropertyName("synonyms_chinese")]
        public string[]? SynonymsChinese { get; init; }

        [JsonPropertyName("season")]
        public string? Season { get; init; }

        [JsonPropertyName("anime")]
        public string? Anime { get; init; }

        [JsonPropertyName("filename")]
        public string? Filename { get; init; }

        [JsonPropertyName("tokenthumb")]
        public string? Tokenthumb { get; init; }
    }
}
