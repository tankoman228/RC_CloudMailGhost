using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CloudMailGhost.Desktop.Singletones;
using CloudMailGhost.Desktop.Views;
using DynamicData;
using MsBox.Avalonia.ViewModels.Commands;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CloudMailGhost.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<FileItemViewModel> Files { get; } = [];
    public static MainViewModel Instance;
    public ICommand CommandSelectKey { get; }
    public ICommand CommandSelectIO { get; }
    public ICommand CommandSelectFake { get; }
    public ICommand CommandSelectDownloads { get; }
    public ICommand CommandHelp { get; }
    public ICommand CommandDebug { get; }
    public ICommand CommandSend { get; }

    public MainViewModel() : base()
    {
        CommandSelectKey        = new RelayCommand(SelectKey);
        CommandSelectIO         = new RelayCommand(SelectIO);
        CommandSelectFake       = new RelayCommand(SelectFake);
        CommandSelectDownloads  = new RelayCommand(SelectDownloads);
        CommandHelp             = new RelayCommand(Help);
        CommandDebug            = new RelayCommand(Debug);
        CommandSend             = new RelayCommand(Send);

        Task.Run(async () =>
        {
            do {
                RescanFiles();
                await Task.Delay(5000);            
            } while (_target != null);
        });

        Instance = this;
    }

    private MainWindow _target => MainWindow.Instance;

    public float Progress { get => progress; set => this.RaiseAndSetIfChanged(ref progress, value); }
    private float progress;

    private void RescanFiles()
    {
        try
        {
            Files.Clear();
            Files.AddRange(Directory.EnumerateFiles(Config.PathToIO).Where(x => x.EndsWith(".png")).Select(y => new FileItemViewModel(y)));
        }
        catch (Exception ex) {}
    }

    #region Commands

    private async void SelectKey(object _)
    {
        // Получаем провайдер хранилищ
        var storageProvider = _target.StorageProvider;
        var fileType = new FilePickerFileType("Text files")
        {
            Patterns = new[] { "*.*" },
            MimeTypes = new[] { "text/plain" }
        };

        var options = new FilePickerOpenOptions
        {
            Title = "Выберите файл с ключом шифрования (до 2 КБ)",
            AllowMultiple = false,
            FileTypeFilter = new[] { fileType }
        };

        // Открываем диалог
        var files = await storageProvider.OpenFilePickerAsync(options);
        if (files.Count == 1)
        {
            var selectedFile = files[0];
            
            await using var stream = await selectedFile.OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            var content = await streamReader.ReadToEndAsync();

            Config.Key = content;
            Config.Save();
        }
    }

    private async void SelectIO(object _)
    {      
        var storageProvider = _target.StorageProvider;

        var options = new FolderPickerOpenOptions
        {
            Title = "Выберите директорию, откуда будет вестись дешифровка и куда будут попадать новые шифруемые файлы",
            AllowMultiple = false
        };

        // Открываем диалог выбора папки
        var folders = await storageProvider.OpenFolderPickerAsync(options);

        if (folders.Any())
        {
            var selectedFolder = folders[0];
            var folderPath = selectedFolder.Path.AbsolutePath;

            Config.PathToIO = folderPath;
            Config.Save();
        }        
    }

    private async void SelectFake(object _)
    {
        var storageProvider = _target.StorageProvider;

        var options = new FolderPickerOpenOptions
        {
            Title = "Выберите директорию с файлами, в которые будут прятаться ваши сообщения",
            AllowMultiple = false
        };

        // Открываем диалог выбора папки
        var folders = await storageProvider.OpenFolderPickerAsync(options);

        if (folders.Any())
        {
            var selectedFolder = folders[0];
            var folderPath = selectedFolder.Path.AbsolutePath;

            Config.PathToFake = folderPath;
            Config.Save();
        }
    }

    private async void SelectDownloads(object _)
    {
        var storageProvider = _target.StorageProvider;

        var options = new FolderPickerOpenOptions
        {
            Title = "Выберите папку, куда будут сохраняться расшифрованные файлы",
            AllowMultiple = false
        };

        // Открываем диалог выбора папки
        var folders = await storageProvider.OpenFolderPickerAsync(options);

        if (folders.Any())
        {
            var selectedFolder = folders[0];
            var folderPath = selectedFolder.Path.AbsolutePath;

            Config.PathToDownloads = folderPath;
            Config.Save();
        }
    }

    private void Help(object _)
    {
        _target.ShowMessage(File.ReadAllText("Assets/for_teapots.txt"));
    }

    private void Debug(object _)
    {
        MainWindow.Instance.ShowMessage($"Константы шифрования: \n" +
            $"Rarefaction\t{CloudMailGhost.Lib.ImageEncoder.Rarefaction}\n" +
            $"MaxColorDifference\t{CloudMailGhost.Lib.ImageEncoder.MaxColorDifference}\n" +
            $"ModFragmentKoef\t{CloudMailGhost.Lib.ImageEncoder.ModFragmentKoef}\n" +
            $"JijkaMinValue\t{CloudMailGhost.Lib.ImageEncoder.JijkaMinValue}\n" +
            $"MinColorDifference\t{CloudMailGhost.Lib.ImageEncoder.MinColorDifference}\n" +
            $"");
    }

    private async void Send(object _)
    {
        try
        {
            // Получаем провайдер хранилищ
            var storageProvider = _target.StorageProvider;
            var fileType = new FilePickerFileType("All files")
            {
                Patterns = new[] { "*.*" }
            };

            var options = new FilePickerOpenOptions
            {
                Title = "Выберите файл, который хотите зашифровать",
                AllowMultiple = false,
                FileTypeFilter = new[] { fileType }
            };

            // Открываем диалог
            var files = await storageProvider.OpenFilePickerAsync(options);
            if (files.Count != 1) return;
            var selectedFileToEncode = files[0];

            options = new FilePickerOpenOptions
            {
                Title = "Выберите файл, в который вы хотите спрятать данные (желательно пожирнее)",
                AllowMultiple = false,
                FileTypeFilter = new[] { fileType },
                SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(new Uri(Config.PathToFake))
            };

            var files2 = await storageProvider.OpenFilePickerAsync(options);
            if (files.Count != 1) return;
            var selectedFileToHide = files2[0];

            await MessageEncoder.Encode(selectedFileToHide.Path.AbsolutePath, selectedFileToEncode.Path.AbsolutePath);
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    #endregion
}
