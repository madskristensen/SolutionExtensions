using Newtonsoft.Json;

namespace SolutionExtensions
{
    public class ExtensionModel
    {
        public string Name { get; set; }
        public string ProductId { get; set; }

        [JsonIgnore]
        public string Category { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
