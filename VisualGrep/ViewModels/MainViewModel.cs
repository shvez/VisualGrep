using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.Serialization;

using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using VisualGrep.Filter;
using VisualGrep.Models;

namespace VisualGrep.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? loadCancellationSource;
    private SemaphoreSlim loadEndEvent = new SemaphoreSlim(1);

    public bool IsFileListSet { get; set; } = true;

    public bool IsFolderSet { get; set; } = true;

    public bool IsFilterSet { get; set; } = true;


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

        this.SearchFilter = "Plug.*\\.";
        this.UseRegExp = true;
    }

    [Reactive]
    public ObservableCollection<LogRecord> LogRecords { get; set; }

    [Reactive]
    public string Status { get; private set; }

    [Reactive] 
    public string FileFilter { get; private set; } = "*.*";

    [Reactive] 
    public string SearchFilter { get; private set; }

    [Reactive] public bool IgnoreCase { get; private set; } = true;

    [Reactive]
    public bool UseRegExp { get; private set; }

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

    public void OnSearchFilterTextInput(object sender, Avalonia.Input.TextInputEventArgs args)
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

    private bool CheckSearchConditions()
    {
        return this.IsFolderSet && this.IsFileListSet && this.IsFilterSet;
    }

    private async Task DoSearch()
    {
        this.StopSearch();

        this.LogRecords.Clear();

        ISearchFilter filter;
        if (this.UseRegExp)
        {
            filter = new RegExpFilter(this.SearchFilter, this.IgnoreCase);
        }
        else
        {
            filter = new SubStringFilter(this.SearchFilter, this.IgnoreCase);
        }

        await this.loadEndEvent.WaitAsync();
        this.loadCancellationSource = new CancellationTokenSource();
        var folder = @"C:\_Work_\_EG_\POCs\VisualGrep\gcfra1022_log\";

        var reader = new FileReader(folder, "*.*");

        int countOfLoaded = 0;
        await foreach (var lr in reader.GetLogRecords(filter).WithCancellation(this.loadCancellationSource.Token))
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
