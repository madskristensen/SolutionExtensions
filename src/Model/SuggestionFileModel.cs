using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SolutionExtensions
{
    public class SuggestionFileModel
    {
        [JsonProperty("extensions")]
        public List<SuggestionModel> Extensions { get; set; }

        public async static Task<SuggestionFileModel> FromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            string fileContent = null;

            using (TextReader file = File.OpenText(fileName))
            {
                fileContent = await file.ReadToEndAsync();
            }

            var obj = JObject.Parse(fileContent);
            SuggestionFileModel fileModel = new SuggestionFileModel { Extensions = new List<SuggestionModel>() };

            foreach (var ext in obj["extensions"])
            {
                SuggestionModel model = new SuggestionModel
                {
                    Name = ((JProperty)ext).Name,
                    ProductId = ext.FirstOrDefault()?["productId"].ToString(),
                    Description = ext.FirstOrDefault()?["description"].ToString(),
                    FileTypes = ext.FirstOrDefault()?["fileTypes"].Values<string>().ToArray(),
                };

                fileModel.Extensions.Add(model);
            }

            return fileModel;
        }
    }
}
