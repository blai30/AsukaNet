using System;
using System.Text.Json.Serialization;

namespace Asuka.Models.Api.Urban
{
    public record UrbanEntry
    {
        [JsonPropertyName("definition")]
        public string Definition { get; init; }

        [JsonPropertyName("permalink")]
        public string Permalink { get; init; }

        [JsonPropertyName("thumbs_up")]
        public int ThumbsUp { get; init; }

        [JsonPropertyName("thumbs_down")]
        public int ThumbsDown { get; init; }

        [JsonPropertyName("author")]
        public string Author { get; init; }

        [JsonPropertyName("word")]
        public string Word { get; init; }

        [JsonPropertyName("written_on")]
        public DateTime WrittenOn { get; init; }

        [JsonPropertyName("example")]
        public string Example { get; init; }
    }
}
