using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.Serialization;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using VisualGrep.Models;

namespace VisualGrep.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        this.LogRecords = [
            new LogRecord 
                { FileName = "f1", LineNumber = 1, Message = "message1" },
            new LogRecord 
                { FileName = "f1", LineNumber = 2, Message = "message2" },
            new LogRecord 
                { FileName = "f1", LineNumber = 3, Message = "message3" },
            new LogRecord 
                { FileName = "f1", LineNumber = 4, Message = "message4" }
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
        var lr = new LogRecord()
        {
            FileName = "f1",
            LineNumber = 1,
            Message = Guid.NewGuid().ToString(),
        };

        this.LogRecords.Add(lr);
    }

}
