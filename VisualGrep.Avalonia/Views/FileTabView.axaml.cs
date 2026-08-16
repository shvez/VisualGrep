using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using VisualGrep.ViewModels;

namespace VisualGrep.Avalonia.Views;

public partial class FileTabView : UserControl
{
    private FileTabViewModel? currentTab;

    public FileTabView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (this.currentTab != null)
        {
            this.currentTab.PropertyChanged -= this.OnTabPropertyChanged;
        }

        this.currentTab = this.DataContext as FileTabViewModel;
        if (this.currentTab != null)
        {
            this.currentTab.PropertyChanged += this.OnTabPropertyChanged;
        }
    }

    private void OnTabPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileTabViewModel.SelectedLineIndex))
        {
            this.ScrollToSelectedLine();
        }
    }

    private void ScrollToSelectedLine()
    {
        if (this.currentTab?.SelectedLineIndex is not { } index)
        {
            return;
        }

        this.FileLinesListBox.SelectedIndex = index;

        var item = this.currentTab.Lines[index];
        try
        {
            this.FileLinesListBox.ScrollIntoView(item);
        }
        catch
        {
            // Ignore if virtualization prevents scrolling.
        }
    }

    private async void EncodingComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (this.DataContext is not FileTabViewModel tab || this.EncodingComboBox.SelectedItem is not EncodingInfo selectedEncoding)
        {
            return;
        }

        if (tab.SelectedEncoding == selectedEncoding)
        {
            return;
        }

        tab.SelectedEncoding = selectedEncoding;
        await tab.ReloadAsync();
    }

    private void OpenFileExternally_Click(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not FileTabViewModel tab || this.VisualRoot is not Window window)
        {
            return;
        }

        if (window.DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.OpenFileService?.OpenFile(tab.FilePath);
        }
    }
}
