using System.Text.Json.Serialization;

namespace Asuka.Models.API.TraceMoe
{
    public class TraceMoeDoc
    {
        [JsonPropertyName("from")]
        public float From { get; set; }

        [JsonPropertyName("to")]
        public float To { get; set; }

        [JsonPropertyName("at")]
        public float At { get; set; }

        [JsonPropertyName("episode")]
        public object? Episode { get; set; }

        [JsonPropertyName("similarity")]
        public float Similarity { get; set; }

        [JsonPropertyName("anilist_id")]
        public int AnilistId { get; set; }

        [JsonPropertyName("mal_id")]
        public int MalId { get; set; }

        [JsonPropertyName("is_adult")]
        public bool IsAdult { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("title_native")]
        public string? TitleNative { get; set; }

        [JsonPropertyName("title_chinese")]
        public string? TitleChinese { get; set; }

        [JsonPropertyName("title_english")]
        public string? TitleEnglish { get; set; }

        [JsonPropertyName("title_romaji")]
        public string? TitleRomaji { get; set; }

        [JsonPropertyName("synonyms")]
        public string[]? Synonyms { get; set; }

        [JsonPropertyName("synonyms_chinese")]
        public string[]? SynonymsChinese { get; set; }

        [JsonPropertyName("season")]
        public string? Season { get; set; }

        [JsonPropertyName("anime")]
        public string? Anime { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("tokenthumb")]
        public string? Tokenthumb { get; set; }
    }
}
