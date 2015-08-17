using Newtonsoft.Json;

namespace SolutionExtensions
{
    public class ExtensionModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonIgnore]
        public string Category { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
