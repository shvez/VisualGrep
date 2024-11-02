using System;
using Avalonia.Controls;
using Avalonia.Input;
using VisualGrep.ViewModels;

namespace VisualGrep.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void AutoCompleteBox_OnDropDownOpened(object? sender, EventArgs e)
    {
        
    }

    private void AutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        
    }

    private void RegExpAutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        this.BringIntoView();
    }

    private void FilesAutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
    }

    private async void RegExpInputElement_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (this.DataContext is MainViewModel model && model.ValidateRegExpFilter())
            {
                 await model.DoSearch();
            }
        }
    }
}
