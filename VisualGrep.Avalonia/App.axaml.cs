using System;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using VisualGrep.Avalonia.Services;
using VisualGrep.Avalonia.Views;
using VisualGrep.ViewModels;

namespace VisualGrep.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
            };
            var folderSelectionService = new DesktopSelectFolderService(desktop.MainWindow, Environment.CurrentDirectory);
            var fileSelectionService = new DesktopSelectFilesService(desktop.MainWindow, Environment.CurrentDirectory);

            var view = new MainViewModel()
            {
                FolderSelectionService = folderSelectionService,
                FileSelectionService = fileSelectionService
            };
            desktop.MainWindow.DataContext = view;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
