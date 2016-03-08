using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace SolutionExtensions
{
    internal sealed class ShowMissingCommand
    {
        private readonly Package _package;
        private OleMenuCommand _button;

        private ShowMissingCommand(Package package)
        {
            _package = package;
            SolutionHandler.ExtensionFileFound += ExtensionFileFound;
            SolutionHandler.SolutionClosed += SolutionClosed;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidExtensionCmdSet, PackageIds.cmdShowMissing);
                _button = new OleMenuCommand(async (s, e) => await ShowDialog(s, e), menuCommandID);
                _button.BeforeQueryStatus += _button_BeforeQueryStatus;
                commandService.AddCommand(_button);
            }
        }

        private void _button_BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            button.Enabled = false;

            string solution = VSPackage.GetSolution();

            if (string.IsNullOrEmpty(solution))
                return;

            string fileName = Path.ChangeExtension(solution, Constants.EXTENSION);

            button.Enabled = File.Exists(fileName);
        }

        private void SolutionClosed(object sender, EventArgs e)
        {
            if (_button != null)
            {
                _button.Enabled = false;
            }
        }

        private void ExtensionFileFound(object sender, ExtensionFileEventArgs e)
        {
            if (_button != null)
            {
                _button.Enabled = e.Model != null;
            }
        }

        public static ShowMissingCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new ShowMissingCommand(package);
        }

        private async System.Threading.Tasks.Task ShowDialog(object sender, EventArgs e)
        {
            ExtensionFileModel fileModel = await SolutionHandler.Instance.GetCurrentFileModel();

            if (fileModel != null)
            {
                Telemetry.TrackEvent("Show solution specific");
                await ExtensionInstalledChecker.Instance.ShowDialog(fileModel);
            }
        }
    }
}

