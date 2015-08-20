using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionExtensions
{
    class InfoBarService
    {
        private IServiceProvider _serviceProvider;

        private InfoBarService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static InfoBarService Instance { get; private set; }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            Instance = new InfoBarService(serviceProvider);
        }

        public void ShowInfoBar(int suggestions)
        {
            var host = GetInfoBarHost();

            if (host != null)
            {
                string message = $"{suggestions} extensions supporting this file type are found";

                if (suggestions == 1)
                    message = $"{suggestions} extension supporting this file type is found";

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
            InfoBarModel infoBarModel = new InfoBarModel(spans, actions, KnownMonikers.StatusInformation, isCloseButtonVisible: true);

            var factory = _serviceProvider.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            IVsInfoBarUIElement element = factory.CreateInfoBar(infoBarModel);
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
                            //IVsWindowFrame windowFrame = (IVsWindowFrame5)obj;
                            return (IVsInfoBarHost)obj;
                        }
                    }
                }
            }

            return null;
        }
    }
}
