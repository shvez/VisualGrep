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

        public string? GetFolder()
        {
            var options = new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false,
                SuggestedStartLocation = this.window.StorageProvider.TryGetFolderFromPathAsync(this.lastFolder).GetAwaiter().GetResult()
            };
            var result = this.window.StorageProvider.OpenFolderPickerAsync(options).GetAwaiter().GetResult();

            if (result.Count == 0)
            {
                return Environment.CurrentDirectory;
            }

            this.lastFolder = result[0].TryGetLocalPath() ?? this.lastFolder;

            return this.lastFolder;
        }
    }
}
