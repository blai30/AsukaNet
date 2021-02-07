using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Asuka.Models.API.Urban
{
    public class UrbanList
    {
        [JsonPropertyName("list")]
        public List<UrbanEntry> UrbanEntries { get; set; }
    }
}
