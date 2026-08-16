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

    private readonly SourceList<LogRecord> logRecords = new SourceList<LogRecord>();

    public bool IsFileListSet { get; set; } = true;

    public bool IsFolderSet { get; set; } = true;

    public bool IsFilterSet { get; set; } = true;


    public MainViewModel()
    {
        this.logRecords.AddRange([
            new LogRecord 
                { FileName = "f1", LineNumber = "1", Message = "message1" },
            new LogRecord 
                { FileName = "f1", LineNumber = "2", Message = "message2" },
            new LogRecord 
                { FileName = "f1", LineNumber = "3", Message = "message3" },
            new LogRecord 
                { FileName = "f1", LineNumber = "4", Message = "message4" }
        ]);

        this.logRecords.Connect().Bind(out var lr).Subscribe();
        this.LogRecords = lr;

        this.FileTabs = [];

        this.FolderSelectCommand = ReactiveCommand.Create(this.OnFolderSelectCommand);
        this.FileSelectCommand = ReactiveCommand.Create(this.OnFileSelectCommand);
        this.SearchCommand = ReactiveCommand.Create(this.OnSearchCommand);
        this.StopCommand = ReactiveCommand.Create(this.OnStopCommand);

        this.OpenSelectedFileCommand = ReactiveCommand.Create(this.OnOpenSelectedFileCommand);
        this.CloseTabCommand = ReactiveCommand.Create<FileTabViewModel>(this.OnCloseTabCommand);
        this.CloseOtherTabsCommand = ReactiveCommand.Create<FileTabViewModel>(this.OnCloseOtherTabsCommand);
        this.CloseAllTabsCommand = ReactiveCommand.Create<FileTabViewModel>(_ => this.OnCloseAllTabsCommand());

        this.SearchFilter = "";
        this.UseRegExp = true;
        this.Folder = Environment.CurrentDirectory;

        this.WhenAnyValue(x => x.SelectedLogRecord)
            .Subscribe(this.OnSelectedLogRecordChanged);

        this.WhenAnyValue(x => x.SelectedFileTab)
            .Subscribe(this.OnSelectedFileTabChanged);
    }

    private void OnSelectedFileTabChanged(FileTabViewModel? tab)
    {
        foreach (var fileTab in this.FileTabs)
        {
            fileTab.IsSelected = fileTab == tab;
        }
    }

    [Reactive]
    public ReadOnlyObservableCollection<LogRecord> LogRecords { get; set; }

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

    [Reactive]
    public ObservableCollection<FileTabViewModel> FileTabs { get; set; }

    [Reactive]
    public FileTabViewModel? SelectedFileTab { get; set; }

    [Reactive]
    public bool IsFileTabsVisible { get; set; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> FolderSelectCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> FileSelectCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> StopCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<Unit, Unit> OpenSelectedFileCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<FileTabViewModel, Unit> CloseTabCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<FileTabViewModel, Unit> CloseOtherTabsCommand { get; }

    [IgnoreDataMember]
    public ReactiveCommand<FileTabViewModel, Unit> CloseAllTabsCommand { get; }

    public ISelectFolderService? FolderSelectionService { get; set; }
    public ISelectFilesService? FileSelectionService { get; set; }

    public IOpenFileService? OpenFileService { get; set; }

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

    private async void OnSelectedLogRecordChanged(LogRecord? record)
    {
        if (record == null)
        {
            return;
        }

        var filePath = Path.Combine(this.Folder, record.FileName);

        var tab = this.FileTabs.FirstOrDefault(t => string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        if (tab == null)
        {
            tab = new FileTabViewModel(filePath);
            this.FileTabs.Add(tab);
        }

        if (int.TryParse(record.LineNumber, out var lineNumber))
        {
            await tab.LoadLinesAsync(lineNumber);
        }
        else
        {
            await tab.LoadLinesAsync();
        }

        this.SelectedFileTab = tab;
        this.IsFileTabsVisible = true;
        this.OnSelectedFileTabChanged(tab);
    }

    private void OnOpenSelectedFileCommand()
    {
        var filePath = this.GetSelectedFilePath();
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        this.OpenFileService?.OpenFile(filePath);
    }

    public string? GetSelectedFilePath()
    {
        if (this.SelectedLogRecord != null)
        {
            return Path.Combine(this.Folder, this.SelectedLogRecord.FileName);
        }

        return this.SelectedFileTab?.FilePath;
    }

    private void OnCloseTabCommand(FileTabViewModel tab)
    {
        var index = this.FileTabs.IndexOf(tab);
        this.FileTabs.Remove(tab);

        if (this.SelectedFileTab == tab)
        {
            if (this.FileTabs.Count == 0)
            {
                this.SelectedFileTab = null;
            }
            else
            {
                var newIndex = index < this.FileTabs.Count ? index : this.FileTabs.Count - 1;
                this.SelectedFileTab = this.FileTabs[newIndex];
            }
        }

        this.IsFileTabsVisible = this.FileTabs.Count > 0;
    }

    private void OnCloseOtherTabsCommand(FileTabViewModel tab)
    {
        var toRemove = this.FileTabs.Where(t => t != tab).ToList();
        foreach (var item in toRemove)
        {
            this.FileTabs.Remove(item);
        }

        this.SelectedFileTab = tab;
        this.IsFileTabsVisible = this.FileTabs.Count > 0;
    }

    private void OnCloseAllTabsCommand()
    {
        this.FileTabs.Clear();
        this.SelectedFileTab = null;
        this.IsFileTabsVisible = false;
    }

    public async Task DoSearch()
    {
        this.StopSearch();

        this.logRecords.Clear();

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

        bool first = true;

        int countOfLoaded = 0;
        await foreach (var lr in reader.GetLogRecords(filter).WithCancellation(this.loadCancellationSource.Token))
        {
          if (first)
          {
              this.DataGridService.UpdateColumns(lr[1].AdditionalInfo.Keys);
              first = false;
          }

          this.logRecords.AddRange(lr);
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
