using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionExtensions
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [Guid(GuidList.guidVSPackageString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        public const string Version = "1.0";
        private int _errorCount;

        private static DTE2 DTE { get; set; }

        protected override void Initialize()
        {
            // Initialize shared components
            DTE = GetService(typeof(DTE)) as DTE2;
            Logger.Initialize(this, Constants.VSIX_NAME);
            Settings.Initialize(this);
            SolutionHandler.Initialize(DTE);

            // Initialize other components
            var repository = (IVsExtensionRepository)GetService(typeof(SVsExtensionRepository));
            var manager = (IVsExtensionManager)GetService(typeof(SVsExtensionManager));
            ExtensionInstalledChecker.Initialize(repository, manager);
            ExtensionInstalledChecker.Instance.InstallProgress += ShowInstallProgress;
            ShowDialogCommand.Initialize(this);

            base.Initialize();
        }

        public static string GetSolution()
        {
            if (DTE.Solution == null)
                return null;

            return DTE.Solution.FullName;
        }

        private void ShowInstallProgress(object sender, InstallerProgressEventArgs e)
        {
            string message = "Downloading and installing extensions...";

            if (e.AmountInstalled > 0)
            {
                message = $"Installed {e.Name}";
            }
            else if (e.Error != null)
            {
                _errorCount += 1;
                Logger.Log(e.Error);
                message = $"Error installing {e.Name}";
            }

            DTE.StatusBar.Progress(true, message, e.AmountInstalled + 1, e.Total + 1);

            if (e.AmountInstalled == e.Total)
            {
                DTE.StatusBar.Progress(false);

                // Only prompt for restart if at least one extension was installed.
                if (_errorCount != e.AmountInstalled)
                    PromptForRestart();
            }
        }

        private static void PromptForRestart()
        {
            string prompt = "You must restart Visual Studio for the extensions to be loaded.\r\rRestart now?";
            var result = MessageBox.Show(prompt, Constants.VSIX_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start($"\"{DTE.FullName}\"", $"\"{DTE.Solution.FullName}\"");
                DTE.ExecuteCommand("File.Exit");
            }
        }
    }
}
