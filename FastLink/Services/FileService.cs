using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink.Services
{
    public class FileService
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true
        };

        public static ObservableCollection<RowItem> LoadRows(string filePath)
        {
            if (!File.Exists(filePath)) return [];
            try
            {
                var items = JsonSerializer.Deserialize<ObservableCollection<RowItem>>(File.ReadAllText(filePath));
                return items ?? [];
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return [];
        }

        public static void SaveRows(string filePath, ObservableCollection<RowItem> rows)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(rows, WriteOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
