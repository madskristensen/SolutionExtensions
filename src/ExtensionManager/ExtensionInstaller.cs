using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.ExtensionManager;

namespace SolutionExtensions
{
    public class ExtensionInstaller
    {
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;

        public ExtensionInstaller(IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _repository = repository;
            _manager = manager;

            _repository.DownloadCompleted += DownloadCompleted;
            _manager.InstallCompleted += InstallCompleted;
        }

        public void InstallExtensions(IEnumerable<ExtensionModel> extensionModels)
        {
            foreach (ExtensionModel model in extensionModels)
            {
                GalleryEntry entry = _repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                        .Where(e => e.VsixID == model.ProductId)
                                                        .AsEnumerable()
                                                        .FirstOrDefault();

                if (entry != null)
                {
                    _repository.DownloadAsync(entry);
                }
            }
        }

        private void DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            OnDownloadComplete(e);
            _manager.InstallAsync(e.Payload, false);
        }

        private void InstallCompleted(object sender, InstallCompletedEventArgs e)
        {
            OnInstallComplete(e);
            //_manager.Uninstall(e.Extension);
        }

        private void OnDownloadComplete(DownloadCompletedEventArgs e)
        {
            if (DownloadComplete != null)
            {
                DownloadComplete(this, e);
            }
        }

        private void OnInstallComplete(InstallCompletedEventArgs e)
        {
            if (InstallComplete != null)
            {
                InstallComplete(this, e);
            }
        }

        public event EventHandler<DownloadCompletedEventArgs> DownloadComplete;
        public event EventHandler<InstallCompletedEventArgs> InstallComplete;

    }
}
