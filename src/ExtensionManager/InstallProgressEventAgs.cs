using System;
using Microsoft.VisualStudio.ExtensionManager;

namespace SolutionExtensions
{
    public class InstallerProgressEventArgs : EventArgs
    {

        public InstallerProgressEventArgs(InstallCompletedEventArgs evtArgs, int total, int amountInstalled)
        {
            if (evtArgs != null)
            {
                Name = evtArgs.Extension.Header.Name;
                Error = evtArgs.Error;
            }

            Total = total;
            AmountInstalled = amountInstalled;
        }

        public Exception Error { get; set; }

        public string Name { get; set; }

        public int Total { get; set; }

        public int AmountInstalled { get; set; }
    }
}