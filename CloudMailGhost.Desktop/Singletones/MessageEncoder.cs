using CloudMailGhost.Lib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Desktop.Singletones
{
    internal class MessageEncoder
    {
        public static void Encode(string fileFake, string fileReal)
        {
            var fake = ImageLoader.LoadImageFromFile(fileFake);

            var fileBytes = File.ReadAllBytes(fileReal);
            var fileNameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(fileReal));

            byte[] toWrite =
            [
                ..BitConverter.GetBytes(fileNameBytes.Length), // int
                ..fileNameBytes,
                 ..BitConverter.GetBytes(fileBytes.Length), // int
                ..fileBytes,
            ];

            if (fake.Pixels.Length / ImageEncoder.Rarefaction - toWrite.Length < 0)
                throw new Exception($"Нужно больше пикселей, этого файла хватит только ~ на {(float)fake.Pixels.Length / toWrite.Length / ImageEncoder.Rarefaction}%");

            toWrite = [
                ..toWrite,
                ..new byte[fake.Pixels.Length / ImageEncoder.Rarefaction - toWrite.Length]
            ];

            var resImage = ImageEncoder.EncodeDataV1(fake, Config.Key, toWrite);

            var fn = fileFake.Split("/");
            var ff = fn[fn.Length - 1];
            fn = ff.Split("\\");
            ff = fn[fn.Length - 1];

            resImage.Height = fake.Height;
            resImage.Width = fake.Width;

            ImageLoader.SaveImageToFile(resImage, Config.PathToIO + "/" + ff);            
        }
    }
}
