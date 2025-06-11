using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using FastLink.Utils;

namespace FastLink.Services
{
    public class FileService
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        public static ObservableCollection<T> LoadRows<T>(string filePath)
        {
            if (!File.Exists(filePath)) return [];
            try
            {
                var json = File.ReadAllText(filePath);
                var items = JsonConvert.DeserializeObject<ObservableCollection<T>>(json, Settings);
                return items ?? [];
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return [];
        }

        public static void SaveRows<T>(string filePath, ObservableCollection<T> rows)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir) && !string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(rows, Settings);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
