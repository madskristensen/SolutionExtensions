using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ExtensionManager;

namespace SolutionExtensions
{
    public class ExtensionInstalledChecker
    {
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;

        private ExtensionInstalledChecker(IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _repository = repository;
            _manager = manager;

            SolutionHandler.ExtensionFileFound += ExtensionFileFound;
        }

        public static ExtensionInstalledChecker Instance { get; private set; }

        public static void Initialize(IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            Instance = new ExtensionInstalledChecker(repository, manager);
        }

        private async void ExtensionFileFound(object sender, ExtensionFileEventArgs e)
        {
            string solution = VSPackage.GetSolution();

            if (!Settings.IsSolutionIgnored())
            {
                await ShowDialog(e.Model);
            }
        }

        public async Task ShowDialog(ExtensionFileModel model)
        {
            var missingExtensions = await GetMissingExtensions(model);
            InstallerDialog dialog = new InstallerDialog(model, missingExtensions);
            dialog.NeverShowAgainForSolution = Settings.IsSolutionIgnored();

            var result = dialog.ShowDialog();

            Settings.IgnoreSolution(dialog.NeverShowAgainForSolution);

            if (!result.HasValue || !result.Value)
                return;
           
            ExtensionInstaller installer = new ExtensionInstaller(_repository, _manager);
            await installer.InstallExtensions(dialog.SelectedExtensions);
        }

        private async Task<IEnumerable<ExtensionModel>> GetMissingExtensions(ExtensionFileModel model)
        {
            return await Task.Run(() =>
            {
                List<ExtensionModel> models = new List<ExtensionModel>();
                var installedExtensions = GetInstalledExtensions();

                var extensions = model.Extensions.SelectMany(e => e.Value);

                foreach (var extension in extensions)
                {
                    var installed = installedExtensions.FirstOrDefault(ins => ins.Header.Identifier.Equals(extension.ProductId, StringComparison.OrdinalIgnoreCase));

                    if (installed == null)
                        models.Add(extension);
                }

                return models;
            });
        }

        public IEnumerable<IInstalledExtension> GetInstalledExtensions()
        {
            return _manager.GetInstalledExtensions().Where(e => !e.Header.SystemComponent);
        }
    }
}
