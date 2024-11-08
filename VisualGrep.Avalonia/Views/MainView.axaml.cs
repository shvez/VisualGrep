﻿using System;
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

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (this.DataContext is MainViewModel model)
            {
                model.StopSearch();
            }

            return;
        }

        base.OnKeyUp(e);
    }
}
