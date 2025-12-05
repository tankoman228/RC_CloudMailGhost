using CloudMailGhost.Desktop.Views;
using System;
using System.IO;
using System.Text.Json;


namespace CloudMailGhost.Desktop.Singletones
{
    internal class Config
    {
        internal static string PathToIO { get; set; } = "/";
        internal static string PathToFake { get; set; } = "/";
        internal static string PathToDownloads { get; set; } = "/";
        internal static string Key { get; set; }


        private static string ConfigFilePath;

        public static void ReadConfig()
        {
            // Определяем путь к файлу конфигурации в папке с программой
            var appDirectory = AppContext.BaseDirectory;
            ConfigFilePath = Path.Combine(appDirectory, "config.json");

            try
            {
                if (!File.Exists(ConfigFilePath)) throw new FileNotFoundException();
                
                var json = File.ReadAllText(ConfigFilePath);
                var configData = JsonSerializer.Deserialize<ConfigDataDTO>(json);

                if (configData != null)
                {
                    PathToIO = configData.PathToIO;
                    PathToFake = configData.PathToFake;
                    PathToDownloads = configData.PathToDownloads;
                    Key = configData.Key;
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError(ex);
            }
        }

        internal static void Save()
        {
            try
            {
                var configData = new ConfigDataDTO
                {
                    PathToIO = PathToIO,
                    PathToFake = PathToFake,
                    PathToDownloads = PathToDownloads,
                    Key = Key
                };

                var json = JsonSerializer.Serialize(configData, new JsonSerializerOptions
                {
                    WriteIndented = true, // Читабельный формат
                });

                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError(ex);
            }
        }

        
        private class ConfigDataDTO // Класс для сериализации в JSON
        {
            public string? PathToIO { get; set; }
            public string? PathToFake { get; set; }
            public string? PathToDownloads { get; set; }
            public string? Key { get; set; }
        }
    }
}
