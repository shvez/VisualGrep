using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Input;

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

    public void AddColumns(string[] names)
    {
        //this.DataGridControl.Columns.Clear();

        //foreach (var name in names)
        //{
        //    var c = new DataGridTextColumn
        //    {
        //        Header = name
        //    };
        //    this.DataGridControl.Columns.Add(c);
        //}
    }

    public void Clear()
    {
        this.DataGridControl.Columns.Clear();
    }

    public void AddEntries(LogRecord[] entries)
    {
        foreach (var logRecord in entries)
        {
            this.logRecords.AddRange(entries);
        }
    }

    public int AddEntry(LogRecord entry)
    {
     //   this.logRecords.Add(entry);
//        return this.logRecords.Count - 1;
        return 0;
  }

    public void ModifyEntry(int pos, LogRecord entry)
    {
//        this.logRecords[pos] = entry;
    }
}
