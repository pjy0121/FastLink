using System.IO;
using System.Text.Json;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink.Services
{
    public static class SettingsService
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true
        };

        public static string SettingsPath =>
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "FastLink", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, WriteOptions));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
