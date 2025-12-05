using Avalonia.Controls;
using CloudMailGhost.Desktop.Singletones;

namespace CloudMailGhost.Desktop.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        Instance = this;
        this.Closed += MainWindow_Closed;

        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Config.ReadConfig();
        if (
            string.IsNullOrEmpty(Config.PathToFake) || 
            string.IsNullOrEmpty(Config.PathToDownloads) || 
            string.IsNullOrEmpty(Config.PathToIO) || 
            string.IsNullOrEmpty(Config.Key))
        {
            this.ShowMessage("Настройте программу, иначе она не будет работать! Выберите все настройки (слева вверху)");
        }
    }

    private void MainWindow_Closed(object? sender, System.EventArgs e)
    {
        Instance = null;
    }
}
