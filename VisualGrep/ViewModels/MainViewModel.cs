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
        this.SearchCommand = ReactiveCommand.Create(this.OnSearchCommand);
        this.StopCommand = ReactiveCommand.Create(this.OnStopCommand);

        this.Status = "text";
    }

    [Reactive]
    public ObservableCollection<LogRecord> LogRecords { get; set; }

    [Reactive]
    public string Status { get; private set; }

    [Reactive]
    public LogRecord SelectedLogRecord { get; set; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> FolderSelectCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> StopCommand { get; }

    private async void OnFolderSelectCommand()
    {
    }

    private void OnStopCommand()
    {
        this.StopSearch();
    }

    private void StopSearch()
    {
        if (this.loadCancellationSource != null)
        {
            this.loadCancellationSource.Cancel();
        }
    }

    private async void OnSearchCommand()
    {
        await this.DoSearch();
    }

    private async Task DoSearch()
    {
        this.StopSearch();

        this.LogRecords.Clear();

        await this.loadEndEvent.WaitAsync();
        this.loadCancellationSource = new CancellationTokenSource();
        var folder = @"C:\_Work_\_EG_\POCs\VisualGrep\gcfra1022_log\";

        var reader = new FileReader(folder, "*.*");

        int countOfLoaded = 0;
        await foreach (var lr in reader.GetLogRecords().WithCancellation(this.loadCancellationSource.Token))
        {
            this.LogRecords.AddRange(lr);
            countOfLoaded += lr.Count;

            this.Status = $"Found {countOfLoaded} matching lines";
        }

        this.loadCancellationSource.Dispose();
        this.loadCancellationSource = null;
        this.loadEndEvent.Release();
    }

}
