using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Data;
using Avalonia.Threading;

using DynamicData;

using VisualGrep.Models;
using VisualGrep.Services;
using VisualGrep.ViewModels;

namespace VisualGrep.Avalonia.Views;

public partial class MainView : UserControl, IDataGrid
{
    private ReadOnlyObservableCollection<LogRecord> logRecords;
    private MainViewModel? currentModel;

    public MainView()
    {
        InitializeComponent();

    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (this.DataContext is MainViewModel model)
        {
            this.currentModel = model;
            model.DataGridService = this;
            this.logRecords = model.LogRecords;
            model.PropertyChanged += this.OnModelPropertyChanged;
        }
    }

    private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedFileTab))
        {
            Dispatcher.UIThread.Post(this.ScrollSelectedLineIntoView, DispatcherPriority.Background);
        }
    }


    private void AutoCompleteBox_OnDropDownOpened(object? sender, EventArgs e)
    {

    }

    private async void RegExpInputElement_OnKeyUp(object? sender, KeyEventArgs e)
    {
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.F4 && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            Environment.Exit(0);
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (this.DataContext is MainViewModel model)
            {
                model.StopSearch();
            }

            return;
        }

        if (e.Source is TextBox tb && tb.Parent?.Parent == this.RegExpTextBox && e.Key == Key.Enter)
        {
            if (this.DataContext is MainViewModel model && model.ValidateRegExpFilter())
            {
                var _ = model.DoSearch();
            }
        }
        base.OnKeyUp(e);
    }

    private FileTabView? GetSelectedTabView()
    {
        var host = this.FindControl<ItemsControl>("FileTabsHost");
        if (host == null || this.currentModel?.SelectedFileTab == null)
        {
            return null;
        }

        foreach (var container in host.GetRealizedContainers())
        {
            if (container is ContentPresenter { Content: FileTabViewModel vm } presenter && vm == this.currentModel.SelectedFileTab)
            {
                return presenter.Child as FileTabView;
            }
        }

        return null;
    }

    public void UpdateColumns(IEnumerable<string> additionalColumnNames)
    {
        var dataGrid = this.DataGridControl;
        dataGrid.Columns.Clear();

        // Базовые столбцы
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "FileName", 
            Binding = new Binding("FileName"),
            CanUserResize = true
        });
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "LineNumber", 
            Binding = new Binding("LineNumber"),
            CanUserResize = true 
        });
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Message", 
            Binding = new Binding("Message"),
            Width = new DataGridLength(400, DataGridLengthUnitType.Star),
            CanUserResize = true
        });

        // Dynamic columns from AdditionalInfo
        foreach (var columnName in additionalColumnNames)
        {
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = columnName,
                Binding = new Binding($"AdditionalInfo[{columnName}]"),
                CanUserResize = true,
            });
        }
    }

    public void ScrollSelectedLineIntoView()
    {
        Dispatcher.UIThread.Post(
            () =>
            {
                if (this.currentModel?.SelectedFileTab?.SelectedLineIndex is not { } index)
                {
                    return;
                }

                var fileTabView = this.GetSelectedTabView();
                if (fileTabView?.FindControl<DataGrid>("FileLinesDataGrid") is not { } dataGrid || dataGrid.Columns.Count == 0)
                {
                    return;
                }

                if (index < 0 || index >= this.currentModel.SelectedFileTab.Lines.Count)
                {
                    return;
                }

                dataGrid.SelectedIndex = index;
                var item = this.currentModel.SelectedFileTab.Lines[index];

                try
                {
                    dataGrid.ScrollIntoView(item, dataGrid.Columns[0]);
                }
                catch
                {
                    // Ignore if virtualization prevents scrolling.
                }
            },
            DispatcherPriority.Background);
    }
}
