using System.IO;

namespace FastLink.Models
{
    public class AppSettings
    {
        public string SaveFilePath { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "FastLink", "fastlink_rows.json");
        public string BaseModifier { get; set; } = "Control,Shift";
        public string AddHotkey { get; set; } = "F1";
        public string QuickViewHotkey { get; set; } = "Space";
        public bool AutoStart { get; set; } = false;
    }
}
