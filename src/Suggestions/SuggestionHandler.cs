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

        public async Task<SuggestionResult> GetSuggestions(string fileType)
        {
            var fileModel = await GetCurrentFileModel();
            IEnumerable<string> fileTypes;
            var suggestions = GetSuggestedExtensions(fileModel, fileType, out fileTypes);

            if (!suggestions.Any())
                return null;

            var extensions = await GetMissingExtensions(suggestions);

            SuggestionResult result = new SuggestionResult {
                Extensions = extensions,
                Matches = fileTypes
            };

            return result;
        }

        public async Task<SuggestionFileModel> GetCurrentFileModel()
        {
            if (_model == null)
            {
                string assembly = Assembly.GetExecutingAssembly().Location;
                string folder = Path.GetDirectoryName(assembly);
                string fileName = Path.Combine(folder, "JSON\\Schema\\", Constants.SUGGESTIONS_FILENAME);

                _model = await SuggestionFileModel.FromFile(fileName);
            }

            return _model;
        }

        private static IEnumerable<SuggestionModel> GetSuggestedExtensions(SuggestionFileModel fileModel, string fileType, out IEnumerable<string> hits)
        {
            List<SuggestionModel> list = new List<SuggestionModel>();
            List<string> matches = new List<string>();

            foreach (SuggestionModel model in fileModel.Extensions)
            {
                string match = model.FileTypes.FirstOrDefault(ft => fileType.EndsWith(ft, StringComparison.Ordinal));

                if (!string.IsNullOrEmpty(match))
                {
                    matches.Add(match);
                    list.Add(model);
                }
            }

            hits = matches;
            return list;
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
