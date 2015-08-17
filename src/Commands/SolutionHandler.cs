using System;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace SolutionExtensions
{
    class SolutionHandler
    {
        private DTE2 _dte;
        private SolutionEvents _events;

        private SolutionHandler(DTE2 dte)
        {
            _dte = dte;
            _events = _dte.Events.SolutionEvents;
            _events.Opened += OnSolutionOpened;
            _events.AfterClosing += OnSolutionClosed;
        }

        public static SolutionHandler Instance { get; private set; }

        public static void Initialize(DTE2 dte)
        {
            Instance = new SolutionHandler(dte);
        }

        public ExtensionFileModel GetCurrentFileModel()
        {
            if (_dte.Solution == null || string.IsNullOrEmpty(_dte.Solution.FullName))
                return null;

            string solutionFolder = Path.GetDirectoryName(_dte.Solution.FullName);
            string configPath = Path.Combine(solutionFolder, Constants.FILENAME);

            return ExtensionFileModel.FromFile(configPath);
        }

        private void OnSolutionOpened()
        {
            if (ExtensionFileFound != null)
            {
                ExtensionFileModel model = GetCurrentFileModel();

                if (model != null)
                {
                    ExtensionFileFound(this, new ExtensionFileEventArgs(model));
                }
            }
        }

        private void OnSolutionClosed()
        {
            if (SolutionClosed != null)
            {
                SolutionClosed(this, EventArgs.Empty);
            }
        }

        public static event EventHandler<ExtensionFileEventArgs> ExtensionFileFound;
        public static event EventHandler SolutionClosed;
    }
}
