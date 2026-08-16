using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DynamicData;
using ReactiveUI.Fody.Helpers;

using VisualGrep.Models;

namespace VisualGrep.ViewModels
{
    public class EncodingInfo
    {
        public required string Name { get; init; }

        public required Encoding Encoding { get; init; }
    }

    public class FileTabViewModel : ViewModelBase
    {
        private readonly List<string> loadedLines = [];
        private int? lastSelectedLineNumber;
        private Encoding? lastUsedEncoding;

        public FileTabViewModel(string filePath)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this.Lines = [];

            this.AvailableEncodings =
            [
                new EncodingInfo { Name = "ANSI", Encoding = Encoding.Default },
                new EncodingInfo { Name = "UTF-8", Encoding = Encoding.UTF8 },
                new EncodingInfo { Name = "UTF-16", Encoding = Encoding.Unicode },
                new EncodingInfo { Name = "UTF-16 BE", Encoding = Encoding.BigEndianUnicode }
            ];

            this.SelectedEncoding = this.AvailableEncodings[0];
        }

        public string FilePath { get; }

        public string FullFilePath => this.FilePath;

        public string FileName { get; }

        public ObservableCollection<string> Lines { get; }

        public ObservableCollection<EncodingInfo> AvailableEncodings { get; }

        [Reactive]
        public EncodingInfo? SelectedEncoding { get; set; }

        [Reactive]
        public int? SelectedLineIndex { get; set; }

        [Reactive]
        public bool IsSelected { get; set; }

        public async Task LoadLinesAsync(int? selectedLineNumber = null)
        {
            if (selectedLineNumber.HasValue)
            {
                this.lastSelectedLineNumber = selectedLineNumber;
            }

            var currentEncoding = this.SelectedEncoding?.Encoding;
            if (this.loadedLines.Count == 0 || this.lastUsedEncoding != currentEncoding)
            {
                this.lastUsedEncoding = currentEncoding;
                this.loadedLines.Clear();
                this.Lines.Clear();

                if (!File.Exists(this.FilePath))
                {
                    return;
                }

                var encoding = currentEncoding ?? Encoding.UTF8;
                var lines = await Task.Run(() => File.ReadAllLines(this.FilePath, encoding));

                this.loadedLines.AddRange(lines);
                this.Lines.AddRange(lines);
            }

            if (this.lastSelectedLineNumber.HasValue)
            {
                this.EnsureLineSelected(this.lastSelectedLineNumber.Value);
            }
        }

        public async Task ReloadAsync()
        {
            this.loadedLines.Clear();
            await this.LoadLinesAsync(this.lastSelectedLineNumber);
        }

        public bool EnsureLineSelected(int lineNumber)
        {
            if (lineNumber < 0 || lineNumber > this.Lines.Count)
            {
                return false;
            }

            this.SelectedLineIndex = lineNumber;
            this.lastSelectedLineNumber = lineNumber;
            return true;
        }
    }
}
