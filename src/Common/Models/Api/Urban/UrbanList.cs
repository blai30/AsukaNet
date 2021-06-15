using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Asuka.Models.Api.Urban
{
    public record UrbanList
    {
        [JsonPropertyName("list")]
        public List<UrbanEntry> UrbanEntries { get; init; }
    }
}
