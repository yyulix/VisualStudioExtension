using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace FilesInfoExtension
{
    [Guid("dab4f029-9e88-4cca-9368-45eab5d8a375")]
    public class FilesInfoExtensionWindow : ToolWindowPane
    {
        public FilesInfoExtensionWindow() : base(null)
        {
            this.Caption = "Статистика функций";
            this.Content = new FilesInfoExtensionWindowControl();
        }
    }
}
