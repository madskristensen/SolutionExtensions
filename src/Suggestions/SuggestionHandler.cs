using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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

        public SuggestionResult GetSuggestions(string filePath, out IEnumerable<string> fileTypes)
        {
            var fileModel = GetCurrentFileModel();
            var suggestions = GetSuggestedExtensions(fileModel, filePath, out fileTypes);

            if (!suggestions.Any())
                return null;

            SuggestionResult result = new SuggestionResult
            {
                Extensions = suggestions,
                Matches = fileTypes.Where(f => !string.IsNullOrEmpty(f))
            };

            return result;
        }

        public SuggestionFileModel GetCurrentFileModel()
        {
            if (_model == null)
            {
                string assembly = Assembly.GetExecutingAssembly().Location;
                string folder = Path.GetDirectoryName(assembly);
                string fileName = Path.Combine(folder, "JSON\\Schema\\", Constants.SUGGESTIONS_FILENAME);

                _model = SuggestionFileModel.FromFile(fileName);
            }

            return _model;
        }

        private static IEnumerable<IExtensionModel> GetSuggestedExtensions(SuggestionFileModel fileModel, string filePath, out IEnumerable<string> hits)
        {
            List<IExtensionModel> list = new List<IExtensionModel>();
            List<string> matches = new List<string>();
            string fileType = Path.GetFileName(filePath);

            foreach (string key in fileModel.Extensions.Keys)
                foreach (SuggestionModel model in fileModel.Extensions[key])
                {
                    string match = model?.FileTypes?.FirstOrDefault(ft => fileType.EndsWith(ft, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(match) || model.Category == SuggestionFileModel.GENERAL)
                    {
                        if (!string.IsNullOrEmpty(model.TextMatch) && File.Exists(filePath))
                        {
                            string content = File.ReadAllText(filePath);
                            if (!Regex.IsMatch(content, model.TextMatch))
                                continue;
                        }

                        matches.Add(match);
                        list.Add(model);
                    }
                }

            hits = matches;
            return list;
        }

        public IEnumerable<IExtensionModel> GetMissingExtensions(IEnumerable<IExtensionModel> suggestedExtensions)
        {
            List<IExtensionModel> models = new List<IExtensionModel>();
            var installedExtensions = ExtensionInstalledChecker.Instance.GetInstalledExtensions();

            foreach (var extension in suggestedExtensions)
            {
                var installed = installedExtensions.FirstOrDefault(ins => ins.Header.Identifier.Equals(extension.ProductId, StringComparison.OrdinalIgnoreCase));

                if (installed == null)
                    models.Add(extension);
            }

            return models;
        }
    }
}
