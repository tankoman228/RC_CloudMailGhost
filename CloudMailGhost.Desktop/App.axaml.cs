using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using CloudMailGhost.Desktop.ViewModels;
using CloudMailGhost.Desktop.Views;
using System.IO;

namespace CloudMailGhost.Desktop;

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
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        // Алгоритм не тыкать!
        var strings = File.ReadAllLines("Localization/" + "ru.txt");
        for (int i = 2; i < strings.Length; i++)
        {
            var key = strings[i++];
            var value = strings[i++];
            Application.Current.Resources[key] = value;
        }
        

        base.OnFrameworkInitializationCompleted();
    }
}
