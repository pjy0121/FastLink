using Microsoft.Win32;

namespace FastLink.Services
{
    public static class AutoStartService
    {
        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "FastLink";

        public static bool IsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            return key?.GetValue(AppName) != null;
        }

        public static void Set(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (enable)
            {
                string exePath = Environment.ProcessPath;
                if (exePath != null)
                    key.SetValue(AppName, exePath);
            }
            else key.DeleteValue(AppName, false);
        }
    }
}
