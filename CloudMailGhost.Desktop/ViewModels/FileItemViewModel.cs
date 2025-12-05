using CloudMailGhost.Desktop.Core;
using CloudMailGhost.Desktop.Singletones;
using CloudMailGhost.Desktop.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CloudMailGhost.Desktop.ViewModels
{
    public class FileItemViewModel : ViewModelBase
    {
        public string DisplayName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Description { get; set; } = "";
        public ICommand OpenCommand { get; } 

        private string filePath = "";

        public FileItemViewModel() { OpenCommand = new RelayCommand(OpenFile); }

        public FileItemViewModel(string FilePath) : base() {

            OpenCommand = new RelayCommand(OpenFile);
            filePath = FilePath;
            
            var nameSplit = filePath.Split('\\');
            nameSplit = nameSplit[nameSplit.Length - 1].Split('/');

            DisplayName = nameSplit[nameSplit.Length - 1];
            Status = "Статус не указан";

            // TODO: если файл уже дешифрован, обозначить это и открывать иначе
        }

        private void OpenFile(object _)
        {
            try
            {
                MessageDecoder.OpenMessage(filePath);
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError(ex);
            }                   
        }
    }
}
