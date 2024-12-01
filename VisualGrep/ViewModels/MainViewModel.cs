using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using VisualGrep.Filter;
using VisualGrep.Models;
using VisualGrep.Services;

namespace VisualGrep.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? loadCancellationSource;
    private readonly SemaphoreSlim loadEndEvent = new SemaphoreSlim(1);

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
        this.FileSelectCommand = ReactiveCommand.Create(this.OnFileSelectCommand);
        this.SearchCommand = ReactiveCommand.Create(this.OnSearchCommand);
        this.StopCommand = ReactiveCommand.Create(this.OnStopCommand);
        this.StopCommand = ReactiveCommand.Create(this.OnStopCommand);

        this.SearchFilter = "";
        this.UseRegExp = true;
        this.Folder = Environment.CurrentDirectory;
    }

    [Reactive]
    public ObservableCollection<LogRecord> LogRecords { get; set; }

    [Reactive]
    public string Status { get; set; } = "Ready";

    [Reactive]
    public string Folder { get; set; }
    
    [Reactive] 
    public string FileFilter { get; set; } = "*.*";

    [Reactive] 
    public string SearchFilter { get; set; }

    [Reactive] 
    public bool IgnoreCase { get; set; } = true;

    [Reactive]
    public bool UseRegExp { get; set; }

    [Reactive]
    public LogRecord? SelectedLogRecord { get; set; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> FolderSelectCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> FileSelectCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ISelectFolderService? FolderSelectionService { get; set; }
    public ISelectFilesService? FileSelectionService { get; set; }

    public IDataGrid DataGridService { get; set; }

    private void OnFolderSelectCommand()
    {
        if (this.FolderSelectionService == null)
        {
            return;
        }

        this.Folder = this.FolderSelectionService.GetFolder();
        this.FileFilter = "*.*";
    }

    private void OnFileSelectCommand()
    {
        if (this.FileSelectionService == null)
        {
            return;
        }

        (this.Folder, var files) = this.FileSelectionService.GetFileList();

        this.FileFilter = files.Count != 0 ? string.Join(",", files) : "*.*";
    }

    private void OnStopCommand()
    {
        this.StopSearch();
    }

    public void StopSearch()
    {
        if (this.loadCancellationSource != null)
        {
            this.loadCancellationSource.Cancel();
        }
    }

    private void OnSearchCommand()
    {
        _ = this.DoSearch();
    }

    public async Task DoSearch()
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
        var folder = this.Folder;

        var reader = new FileReader(folder, "*.*");

        this.DataGridService.AddColumns(["File", "Line", "Message"]);

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

    public bool ValidateRegExpFilter()
    {
        try
        {
            var options = RegexOptions.Compiled | RegexOptions.Singleline;
            _ = new Regex(this.SearchFilter, options);
        }
        catch (Exception e)
        {
            this.Status = e.Message;
            return false;
        }
        return true;
    }
}
