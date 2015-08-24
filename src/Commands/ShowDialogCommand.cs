using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace SolutionExtensions
{
    internal sealed class ShowDialogCommand
    {
        private readonly Package _package;
        private OleMenuCommand _button;

        private ShowDialogCommand(Package package)
        {
            _package = package;
            SolutionHandler.ExtensionFileFound += ExtensionFileFound;
            SolutionHandler.SolutionClosed += SolutionClosed;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(GuidList.guidExtensionCmdSet, PackageCommands.cmdShowDialog);
                _button = new OleMenuCommand(ShowDialog, menuCommandID);
                _button.BeforeQueryStatus += _button_BeforeQueryStatus;
                commandService.AddCommand(_button);
            }
        }

        private void _button_BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;

            button.Enabled = SolutionHandler.Instance.GetCurrentFileModel() != null;
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

        public static ShowDialogCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new ShowDialogCommand(package);
        }

        private async void ShowDialog(object sender, EventArgs e)
        {
            ExtensionFileModel fileModel = await SolutionHandler.Instance.GetCurrentFileModel();

            if (fileModel != null)
            {
                await ExtensionInstalledChecker.Instance.ShowDialog(fileModel);
            }
        }
    }
}

