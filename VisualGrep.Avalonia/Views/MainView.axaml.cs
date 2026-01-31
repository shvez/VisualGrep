using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Data;

using DynamicData;

using VisualGrep.Models;
using VisualGrep.Services;
using VisualGrep.ViewModels;

namespace VisualGrep.Avalonia.Views;

public partial class MainView : UserControl, IDataGrid
{
    private ReadOnlyObservableCollection<LogRecord> logRecords;

    public MainView()
    {
        InitializeComponent();

    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (this.DataContext is MainViewModel model)
        {
            model.DataGridService = this;
            this.logRecords = model.LogRecords;
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
//                Width = new DataGridLength(30, DataGridLengthUnitType.Star)
            });
        }
    }
}
