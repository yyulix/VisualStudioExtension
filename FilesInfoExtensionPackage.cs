using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FilesInfoExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(FilesInfoExtensionPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(FilesInfoExtensionWindow), MultiInstances = true, Style = VsDockStyle.MDI)]
    public sealed class FilesInfoExtensionPackage : AsyncPackage
    {
        public const string PackageGuidString = "71d32a92-2033-4afc-b243-e66c4e072692";

        #region Package Members
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await FilesInfoExtensionWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}