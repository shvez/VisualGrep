using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using VisualGrep.Services;

namespace VisualGrep.Avalonia.Services
{
    internal class DesktopSelectFilesService : ISelectFilesService
    {
        private readonly Window window;
        private readonly string initialDirectory;

        public DesktopSelectFilesService(Window window, string initialDirectory)
        {
            this.window = window;
            this.initialDirectory = initialDirectory;
        }
        public (string Folder, List<string> Files) GetFileList ()
        {
            var result = this.window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                AllowMultiple = true,
                SuggestedStartLocation = this.window.StorageProvider.TryGetFolderFromPathAsync(this.initialDirectory).GetAwaiter().GetResult(),
            }).GetAwaiter().GetResult();

            if (result.Count == 0)
            {
                return (this.initialDirectory, new List<string>());
            }

            var files = result.Select(x => x.Path.ToString()).ToList();
            var resultDirectory = Path.GetDirectoryName(files.First()) ?? this.initialDirectory;

            for(int i = 0; i < files.Count; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            return (resultDirectory, files);
        }
    }
}
