using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public async static Task<ExtensionFileModel> FromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            string fileContent = null;

            using (TextReader file = File.OpenText(fileName))
            {
                fileContent = await file.ReadToEndAsync();
            }

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
