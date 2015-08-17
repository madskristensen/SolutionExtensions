using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace SolutionExtensions
{
    public static class Settings
    {
        private static SettingsManager _settings;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _settings = new ShellSettingsManager(serviceProvider);
        }

        public static void IgnoreSolution(bool ignore)
        {
            WritableSettingsStore wstore = _settings.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!wstore.CollectionExists(Constants.VSIX_NAME))
                wstore.CreateCollection(Constants.VSIX_NAME);

            string solution = VSPackage.GetSolution();

            if (string.IsNullOrEmpty(solution))
                return;

            string property = GetPropertyName(solution);

            if (ignore)
            {
                wstore.SetInt32(Constants.VSIX_NAME, property, 1);
            }
            else
            {
                wstore.DeleteProperty(Constants.VSIX_NAME, property);
            }
        }

        public static bool IsSolutionIgnored()
        {
            SettingsStore store = _settings.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            string solution = VSPackage.GetSolution();

            if (string.IsNullOrEmpty(solution))
                return false;

            string property = GetPropertyName(solution);

            return store.PropertyExists(Constants.VSIX_NAME, property);
        }

        private static string GetPropertyName(string solution)
        {
            string clean = solution.ToLowerInvariant().Trim();
            byte[] unicodeText = Encoding.UTF8.GetBytes(clean);

            var crypto = new MD5CryptoServiceProvider();
            byte[] result = crypto.ComputeHash(unicodeText);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
