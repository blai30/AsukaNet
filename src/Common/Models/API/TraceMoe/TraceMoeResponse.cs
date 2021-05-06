using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Asuka.Models.API.TraceMoe
{
    public class TraceMoeResponse
    {
        [JsonPropertyName("RawDocsCount")]
        public int RawDocsCount { get; set; }

        [JsonPropertyName("RawDocsSearchTime")]
        public int RawDocsSearchTime { get; set; }

        [JsonPropertyName("ReRankSearchTime")]
        public int ReRankSearchTime { get; set; }

        [JsonPropertyName("CacheHit")]
        public bool CacheHit { get; set; }

        [JsonPropertyName("trial")]
        public int Trial { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("limit_ttl")]
        public int LimitTtl { get; set; }

        [JsonPropertyName("quota")]
        public int Quota { get; set; }

        [JsonPropertyName("quota_ttl")]
        public int QuotaTtl { get; set; }

        [JsonPropertyName("docs")]
        public IEnumerable<TraceMoeDoc>? Docs { get; set; }
    }
}
