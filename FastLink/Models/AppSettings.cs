using System.IO;

namespace FastLink.Models
{
    public class AppSettings
    {
        public string SaveFilePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FastLink", "fastlink_rows.json");
        public string BaseModifier { get; set; } = "Control,Shift";
        public string AddHotkey { get; set; } = "A";
        public string QuickViewHotkey { get; set; } = "Q";
        public bool AutoStart { get; set; } = false;
    }
}
