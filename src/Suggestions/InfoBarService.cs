using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionExtensions
{
    class InfoBarService : IVsInfoBarUIEvents
    {
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;
        private IServiceProvider _serviceProvider;
        private uint _cookie;
        private string _fileType;
        private SuggestionResult _suggestionResult;

        private InfoBarService(IServiceProvider serviceProvider, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
            _manager = manager;
        }

        public static InfoBarService Instance { get; private set; }

        public static void Initialize(IServiceProvider serviceProvider, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            Instance = new InfoBarService(serviceProvider, repository, manager);
        }

        public async void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            string context = actionItem.ActionContext;

            if (context == "install")
            {
                InstallerDialog dialog = new InstallerDialog(_suggestionResult.Extensions);
                dialog.NeverShowAgainForSolution = Settings.IsFileTypeIgnored(_suggestionResult.Matches);

                var dte = _serviceProvider.GetService(typeof(DTE)) as DTE2;
                var hwnd = new IntPtr(dte.MainWindow.HWnd);
                System.Windows.Window window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
                dialog.Owner = window;

                var result = dialog.ShowDialog();

                Settings.IgnoreFileType(_suggestionResult.Matches, dialog.NeverShowAgainForSolution);

                if (!result.HasValue || !result.Value)
                    return;

                ExtensionInstaller installer = new ExtensionInstaller(_serviceProvider, _repository, _manager);
                await installer.InstallExtensions(dialog.SelectedExtensions);
            }
            else if (context == "ignore")
            {
                var props = new Dictionary<string, string> { { "matches", string.Join(", ", _suggestionResult.Matches) } };
                Telemetry.TrackEvent("Ignore", props);
                infoBarUIElement.Close();
                Settings.IgnoreFileType(_suggestionResult.Matches, true);
            }
        }

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            infoBarUIElement.Unadvise(_cookie);
        }

        public void ShowInfoBar(SuggestionResult result, string fileType)
        {
            if (Settings.IsFileTypeIgnored(result.Matches))
                return;

            _fileType = fileType;
            int count = result.Extensions.Count(e => e.Category != SuggestionFileModel.GENERAL);

            var host = GetInfoBarHost();

            if (host != null)
            {
                string matches = string.Join(", ", result.Matches.Distinct());
                string message = $"{count} extensions supporting {matches} files are found";

                if (result.Extensions.Count() == 1)
                    message = $"{count} extension supporting {matches} files is found";

                _suggestionResult = result;
                CreateInfoBar(host, message);
            }
        }

        private void CreateInfoBar(IVsInfoBarHost host, string message)
        {
            InfoBarTextSpan text = new InfoBarTextSpan(message);
            InfoBarHyperlink install = new InfoBarHyperlink("Install extension...", "install");
            InfoBarHyperlink ignore = new InfoBarHyperlink("Ignore file type", "ignore");

            InfoBarTextSpan[] spans = new InfoBarTextSpan[] { text };
            InfoBarActionItem[] actions = new InfoBarActionItem[] { install, ignore };
            InfoBarModel infoBarModel = new InfoBarModel(spans, actions, KnownMonikers.VisualStudioFeedback, isCloseButtonVisible: true);

            var factory = _serviceProvider.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            IVsInfoBarUIElement element = factory.CreateInfoBar(infoBarModel);
            element.Advise(this, out _cookie);
            host.AddInfoBar(element);
        }

        private IVsInfoBarHost GetInfoBarHost()
        {
            IVsUIShell4 uiShell = _serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell4;
            IEnumWindowFrames windowEnumerator;

            uint flags = unchecked(((uint)(__WindowFrameTypeFlags.WINDOWFRAMETYPE_Document)));
            ErrorHandler.ThrowOnFailure(uiShell.GetWindowEnum(flags, out windowEnumerator));

            IVsWindowFrame[] frame = new IVsWindowFrame[1];
            uint fetched = 0;
            int hr = VSConstants.S_OK;
            // Note that we get S_FALSE when there is no more item, so only loop while we are getting S_OK
            while (hr == VSConstants.S_OK)
            {
                // For each tool window, add it to the list
                hr = windowEnumerator.Next(1, frame, out fetched);
                ErrorHandler.ThrowOnFailure(hr);
                if (fetched == 1)
                {
                    if (frame[0].IsVisible() == VSConstants.S_OK)
                    {
                        // We successfully retrieved a window frame, update our lists
                        object obj;
                        frame[0].GetProperty((int)__VSFPROPID7.VSFPROPID_InfoBarHost, out obj);

                        if (obj != null)
                        {
                            return (IVsInfoBarHost)obj;
                        }
                    }
                }
            }

            return null;
        }
    }
}
