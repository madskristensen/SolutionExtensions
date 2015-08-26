using System;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using EnvDTE;
using EnvDTE80;
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
                var menuCommandID = new CommandID(PackageGuids.guidExtensionCmdSet, PackageIds.cmdCreateSolutionExtensions);
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
            string fileName = Path.ChangeExtension(VSPackage.GetSolution(), Constants.EXTENSION);

            if (string.IsNullOrEmpty(fileName))
                return;

            string emptyFile = @"{
  ""extensions"": {
    ""mandatory"": [
      {
        ""name"": ""Web Compiler"",
        ""productId"": ""148ffa77-d70a-407f-892b-9ee542346862""
      }
    ]
  }
}";

            if (!File.Exists(fileName))
                File.WriteAllText(fileName, emptyFile, new UTF8Encoding(true));

            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            dte.ItemOperations.OpenFile(fileName);
        }
    }
}

