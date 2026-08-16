using System;

using Avalonia.Controls;
using VisualGrep.Services;

namespace VisualGrep.Avalonia.Services
{
    public class DesktopOpenFileService : IOpenFileService
    {
        private readonly Window window;

        public DesktopOpenFileService(Window window)
        {
            this.window = window;
        }

        public void OpenFile(string filePath)
        {
            var launcher = this.window.Launcher;
            if (launcher == null)
            {
                return;
            }

            var task = launcher.LaunchUriAsync(new Uri(filePath));
            _ = task.ConfigureAwait(false);
        }
    }
}
