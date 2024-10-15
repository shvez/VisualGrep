using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.Serialization;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using VisualGrep.Models;

namespace VisualGrep.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? loadCancellationSource;
    private SemaphoreSlim loadEndEvent = new SemaphoreSlim(1);

    public MainViewModel()
    {
        this.LogRecords = [
            new LogRecord 
                { FileName = "f1", LineNumber = "1", Message = "message1" },
            new LogRecord 
                { FileName = "f1", LineNumber = "2", Message = "message2" },
            new LogRecord 
                { FileName = "f1", LineNumber = "3", Message = "message3" },
            new LogRecord 
                { FileName = "f1", LineNumber = "4", Message = "message4" }
        ];

        this.FolderSelectCommand = ReactiveCommand.Create(this.OnFolderSelectCommand);
    }

    [Reactive]
    public ObservableCollection<LogRecord> LogRecords { get; set; }

    [Reactive]
    public LogRecord SelectedLogRecord { get; set; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> FolderSelectCommand { get; }

    private async void OnFolderSelectCommand()
    {
        if (this.loadCancellationSource != null)
        {
            this.loadCancellationSource.Cancel();

            this.LogRecords.Clear();
        }


        await this.loadEndEvent.WaitAsync();
        this.loadCancellationSource = new CancellationTokenSource();
        var folder = @"C:\_Work_\_EG_\POCs\VisualGrep\gcfra1022_log\";

        var reader = new FileReader(folder, "*.*");

        await foreach (var lr in reader.GetLogRecords().WithCancellation(this.loadCancellationSource.Token))
        {
            this.LogRecords.AddRange(lr);
        }

        this.loadCancellationSource.Dispose();
        this.loadCancellationSource = null;
        this.loadEndEvent.Release();
    }

}
