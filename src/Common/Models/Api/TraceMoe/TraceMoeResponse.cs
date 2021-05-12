using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Asuka.Models.Api.TraceMoe
{
    public record TraceMoeResponse
    {
        [JsonPropertyName("RawDocsCount")]
        public int RawDocsCount { get; init; }

        [JsonPropertyName("RawDocsSearchTime")]
        public int RawDocsSearchTime { get; init; }

        [JsonPropertyName("ReRankSearchTime")]
        public int ReRankSearchTime { get; init; }

        [JsonPropertyName("CacheHit")]
        public bool CacheHit { get; init; }

        [JsonPropertyName("trial")]
        public int Trial { get; init; }

        [JsonPropertyName("limit")]
        public int Limit { get; init; }

        [JsonPropertyName("limit_ttl")]
        public int LimitTtl { get; init; }

        [JsonPropertyName("quota")]
        public int Quota { get; init; }

        [JsonPropertyName("quota_ttl")]
        public int QuotaTtl { get; init; }

        [JsonPropertyName("docs")]
        public IEnumerable<TraceMoeDoc>? Docs { get; init; }
    }
}
