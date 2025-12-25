using Avalonia.Media;
using CloudMailGhost.Lib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Desktop.ViewModels
{
    public class FakeItemViewModel : ViewModelBase
    {
        public string DisplayName { get; set; } = "";
        public int DataMaxSize { get; set; } = -1;
        public string DisplayDataMaxSize => DataMaxSize.ToString("#,0");
        public string StatusText { get; set; } = "";

        public FakeItemViewModel(string filePath)
        {
            if (filePath.EndsWith(".png"))
            {
                using (var image = Image.Load<Rgba32>(filePath))
                {
                    DataMaxSize = image.Height * image.Width / ImageEncoder.Rarefaction - 16;

                    var nameSplit = filePath.Split('\\');
                    nameSplit = nameSplit[nameSplit.Length - 1].Split('/');
                    DisplayName = nameSplit[nameSplit.Length - 1];



                    if (image.Height * image.Width % 16 != 0)
                    {
                        StatusText = "Не подходит! Кол-во пикселей не кратно 16!";
                    }
                }
            }
        }
    }
}
