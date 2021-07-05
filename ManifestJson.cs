using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ThunderStorePackager
{
    public class ManifestJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("version_number")]
        public string Version { get; set; }

        [JsonPropertyName("dependencies")]
        public IEnumerable<string> DependencyStrings { get; set; }

        [JsonPropertyName("website_url")]
        public string Url { get; set; }
    }
}
