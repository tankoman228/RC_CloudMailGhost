using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Desktop.Singletones
{
    internal class DecodeCacheManager
    {
        
        /// <summary>
        /// Вернёт null, если кэша нет, иначе ссыль на готовый декодированный файл
        /// </summary>
        internal static string? CheckForFile(string pngName)
        {
            var cacheName = Config.PathToDownloads + "/" + pngName + ".cache.decoded";
            if (!File.Exists(cacheName)) return null;
            return File.ReadAllText(cacheName);
        }

        internal static void SetForFile(string pngName, string decodeFileName)
        {
            var cacheName = Config.PathToDownloads + "/" + pngName + ".cache.decoded";
            File.WriteAllText(cacheName, decodeFileName);
        }
    }
}
