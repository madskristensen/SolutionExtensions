using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SolutionExtensions
{
    class SuggestionHandler
    {
        private static SuggestionFileModel _model;

        private SuggestionHandler()
        { }

        public static SuggestionHandler Instance { get; private set; }

        public static void Initialize()
        {
            Instance = new SuggestionHandler();
        }

        public async Task<IEnumerable<SuggestionModel>> GetSuggestions(string fileType)
        {
            var fileModel = await GetCurrentFileModel();
            var suggestions = GetSuggestedExtensions(fileModel, fileType);

            if (!suggestions.Any())
                return Enumerable.Empty<SuggestionModel>();

            return await GetMissingExtensions(suggestions);
        }

        public async Task<SuggestionFileModel> GetCurrentFileModel()
        {
            if (_model == null)
            {
                string assembly = Assembly.GetExecutingAssembly().Location;
                string folder = Path.GetDirectoryName(assembly);
                string fileName = Path.Combine(folder, "Suggestions\\suggestions.json");

                _model = await SuggestionFileModel.FromFile(fileName);
            }

            return _model;
        }

        private static IEnumerable<SuggestionModel> GetSuggestedExtensions(SuggestionFileModel fileModel, string fileType)
        {
            foreach (SuggestionModel model in fileModel.Extensions)
            {
                if (model.FileTypes.Any(ft => fileType.EndsWith(ft, StringComparison.Ordinal)))
                    yield return model;
            }
        }

        private async Task<IEnumerable<SuggestionModel>> GetMissingExtensions(IEnumerable<SuggestionModel> suggestedExtensions)
        {
            return await Task.Run(() =>
            {
                List<SuggestionModel> models = new List<SuggestionModel>();
                var installedExtensions = ExtensionInstalledChecker.Instance.GetInstalledExtensions();

                foreach (var extension in suggestedExtensions)
                {
                    var installed = installedExtensions.FirstOrDefault(ins => ins.Header.Identifier.Equals(extension.ProductId, StringComparison.OrdinalIgnoreCase));

                    if (installed == null)
                        models.Add(extension);
                }

                return models;
            });
        }
    }
}
