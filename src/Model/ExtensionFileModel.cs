using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SolutionExtensions
{
    public class ExtensionFileModel
    {
        [JsonProperty("extensions")]
        public Dictionary<string, IEnumerable<ExtensionModel>> Extensions { get; set; }

        [JsonIgnore]
        public string FileName { get; private set; }

        public void Save(string fileName)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            File.WriteAllText(fileName, json, new UTF8Encoding(false));
        }

        public static ExtensionFileModel FromFile(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            var fileModel = JsonConvert.DeserializeObject<ExtensionFileModel>(fileContent);
            fileModel.FileName = fileName;

            foreach (string category in fileModel.Extensions.Keys)
            {
                foreach (ExtensionModel model in fileModel.Extensions[category])
                {
                    model.Category = category;
                }
            }

            return fileModel;
        }
    }
}
