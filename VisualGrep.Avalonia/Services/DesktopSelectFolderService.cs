using VisualGrep.Services;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;

namespace VisualGrep.Avalonia.Services
{
    public class DesktopSelectFolderService : ISelectFolderService
    {
        private readonly Window window;
        private string lastFolder;

        public DesktopSelectFolderService(Window window, string currentDirectory)
        {
            this.window = window;
            this.lastFolder = currentDirectory;
        }

        public string GetFolder()
        {
            var options = new FolderPickerOpenOptions();
            options.Title = "Select Folder";
            options.AllowMultiple = false;
            options.SuggestedStartLocation = window.StorageProvider.TryGetFolderFromPathAsync(this.lastFolder).GetAwaiter().GetResult();
            var result = window.StorageProvider.OpenFolderPickerAsync(options).GetAwaiter().GetResult();

            if (result == null)
            {
                return Environment.CurrentDirectory;
            }

            this.lastFolder = result[0].Path.ToString();

            return this.lastFolder;
        }
    }
}
