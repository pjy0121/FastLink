using System.IO;
using System.Text.Json;
using FastLink.Utils;
using FastLink.Models;

namespace FastLink.Services
{
    public static class SettingsService
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true
        };

        public static string SettingsPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FastLink", "settings.json");

        public static AppSettings Settings { get; }

        static SettingsService()
        {
            Settings = Load();      // 자동 로드
            Settings.PropertyChanged += (s, e) => Save(Settings);   // 변경 감지 시 자동 저장
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                        return loaded;
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
