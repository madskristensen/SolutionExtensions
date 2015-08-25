using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace SolutionExtensions
{
    internal sealed class ModifyMissingCommand
    {
        private readonly Package _package;

        private ModifyMissingCommand(Package package)
        {
            _package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(GuidList.guidExtensionCmdSet, PackageCommands.cmdCreateSolutionExtensions);
                var button = new OleMenuCommand(ShowDialog, menuCommandID);
                button.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(button);
            }
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;

            button.Enabled = !string.IsNullOrEmpty(VSPackage.GetSolution());
        }

        public static ModifyMissingCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new ModifyMissingCommand(package);
        }

        private void ShowDialog(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Test");
        }
    }
}

