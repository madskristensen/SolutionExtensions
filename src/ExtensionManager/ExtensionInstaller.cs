using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionExtensions
{
    public class ExtensionInstaller
    {
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;
        private InstallerProgress _progress;
        private IServiceProvider _serviceProvider;

        public ExtensionInstaller(IServiceProvider serviceProvider, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            _manager = manager;
        }

        public async Task InstallExtensions(IEnumerable<IExtensionModel> extensionModels)
        {
            bool hasInstalled = false;
            int count = extensionModels.Count();

            _progress = new InstallerProgress(count, $"Downloading and installing {count} extension(s)...");
            _progress.Show();

            await Task.Run(() =>
            {
                foreach (IExtensionModel model in extensionModels)
                {
                    try
                    {
                        GalleryEntry entry = _repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                                 .Where(e => e.VsixID == model.ProductId)
                                                                 .AsEnumerable()
                                                                 .FirstOrDefault();

                        if (!_progress.IsVisible)
                            break;

                        if (entry != null)
                        {
                            IInstallableExtension installable = _repository.Download(entry);

                            if (!_progress.IsVisible)
                                break;

                            _manager.Install(installable, false);
                            hasInstalled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }
            });

            if (_progress.IsVisible)
            {
                _progress.Close();
                _progress = null;
            }

            if (hasInstalled)
                PromptForRestart();
        }

        private void PromptForRestart()
        {
            string prompt = "You must restart Visual Studio for the extensions to be loaded.\r\rRestart now?";
            var result = MessageBox.Show(prompt, Constants.VSIX_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IVsShell4 shell = (IVsShell4)_serviceProvider.GetService(typeof(SVsShell));
                shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);

            }
        }
    }
}
