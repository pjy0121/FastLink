using System.IO;
using System.Windows.Input;
using FastLink.Models;

namespace FastLink.Utils
{
    public static class CommonUtils
    {
        public static void CopyRowPath(RowItem row)
        {
            if (!string.IsNullOrEmpty(row.Path))
                System.Windows.Clipboard.SetText(row.Path);
        }


        public static void OpenRowPath(RowItem row)
        {
            try
            {
                if (row.Type == RowType.Web)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(row.Path) { UseShellExecute = true });
                else if (Directory.Exists(row.Path) || File.Exists(row.Path))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer", $"\"{row.Path}\"") { UseShellExecute = true });
                else throw new Exception("Path does not exist.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open the path.\n\n" + ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
