using System.Collections.Generic;
using Newtonsoft.Json;

namespace SolutionExtensions
{
    public class SuggestionModel : IExtensionModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("fileTypes")]
        public IEnumerable<string> FileTypes { get; set; }

        [JsonIgnore]
        public string Category { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
