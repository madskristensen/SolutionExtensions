using System;
using System.Linq;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ExtensionManager;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SolutionExtensions
{
    internal sealed class ShowSuggestionsCommand
    {
        private readonly Package _package;
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;

        private ShowSuggestionsCommand(Package package, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _package = package;
            _repository = repository;
            _manager = manager;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(GuidList.guidExtensionCmdSet, PackageCommands.cmdShowSuggestions);
                var button = new OleMenuCommand(async (s, e) => await ShowSuggestions(s, e), menuCommandID);
                button.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(button);
            }
        }
        
        public static ShowSuggestionsCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            Instance = new ShowSuggestionsCommand(package, repository, manager);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            button.Enabled = true;
        }

        private async System.Threading.Tasks.Task ShowSuggestions(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            if (dte.ActiveDocument == null || string.IsNullOrEmpty(dte.ActiveDocument.FullName))
            {
                MessageBox.Show("No file is open that has any suggested extensions", Constants.VSIX_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fileName = Path.GetFileName(dte.ActiveDocument.FullName);
            IEnumerable<string> fileTypes;
            var result = SuggestionHandler.Instance.GetSuggestions(fileName, out fileTypes);
            var fileModel = SuggestionHandler.Instance.GetCurrentFileModel().Filter(fileName);            

            if (result != null)
            {
                InstallerDialog dialog = new InstallerDialog(result.Extensions);
                dialog.NeverShowAgainForSolution = Settings.IsFileTypeIgnored(result.Matches);
                var test = dialog.ShowDialog();

                Settings.IgnoreFileType(result.Matches, dialog.NeverShowAgainForSolution);

                if (!test.HasValue || !test.Value)
                    return;

                ExtensionInstaller installer = new ExtensionInstaller(_repository, _manager);
                await installer.InstallExtensions(dialog.SelectedExtensions);
            }
        }
    }
}

