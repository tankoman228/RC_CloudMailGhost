using CloudMailGhost.Lib;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CloudMailGhost.Desktop.Singletones
{
    internal class MessageDecoder
    {
        internal static void OpenMessage(string filename)
        {
            var image = ImageLoader.LoadImageFromFile(filename);
            var data = ImageEncoder.DecodeDataV1(image, Config.Key);

            int sizeName = BitConverter.ToInt32(data.Take(4).ToArray(), 0);
            string fileDecodedName = Encoding.UTF8.GetString(data.Skip(4).Take(sizeName).ToArray());

            int sizeContents = BitConverter.ToInt32(data.Skip(4 + sizeName).Take(4).ToArray(), 0);
            byte[] fileContents = data.Skip(8 + sizeName).Take(sizeContents).ToArray();

            File.WriteAllBytes(Config.PathToDownloads + "/" + fileDecodedName, fileContents);
            Process.Start(new ProcessStartInfo(Config.PathToDownloads + "/" + fileDecodedName) { UseShellExecute = true });
        }
    }
}
