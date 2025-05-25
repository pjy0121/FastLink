using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using FastLink.Utils;

namespace FastLink.Services
{
    public static class ExplorerBrowserService
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public static (double X, double Y) GetForegroundWindowCenter()
        {
            IntPtr fgHwnd = GetForegroundWindow();
            if (fgHwnd != IntPtr.Zero && GetWindowRect(fgHwnd, out RECT rect))
            {
                double centerX = (rect.Left + rect.Right) / 2.0;
                double centerY = (rect.Top + rect.Bottom) / 2.0;
                return (centerX, centerY);
            }
            return (SystemParameters.PrimaryScreenWidth / 2, SystemParameters.PrimaryScreenHeight / 2);
        }

        public static string? GetActiveExplorerPath()
        {
            try
            {
                var shellWindows = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
                var windows = (System.Collections.IEnumerable)shellWindows.GetType().InvokeMember(
                    "Windows",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, shellWindows, null);

                IntPtr foreground = GetForegroundWindow();

                foreach (var window in windows)
                {
                    var hwnd = (IntPtr)Convert.ToInt32(window.GetType().InvokeMember(
                        "HWND",
                        System.Reflection.BindingFlags.GetProperty,
                        null, window, null));

                    if (hwnd == foreground)
                    {
                        // Document 객체 가져오기
                        var doc = window.GetType().InvokeMember(
                            "Document",
                            System.Reflection.BindingFlags.GetProperty,
                            null, window, null);

                        // 선택된 항목 가져오기
                        var selectedItems = doc.GetType().InvokeMember(
                            "SelectedItems",
                            System.Reflection.BindingFlags.InvokeMethod,
                            null, doc, null) as System.Collections.IEnumerable;

                        if (selectedItems != null)
                        {
                            foreach (var item in selectedItems)
                            {
                                string? path = item.GetType().InvokeMember(
                                    "Path",
                                    System.Reflection.BindingFlags.GetProperty,
                                    null, item, null) as string;
                                if (!string.IsNullOrEmpty(path))
                                    return path; // 첫 번째 선택 항목 경로 반환
                            }
                        }

                        // 선택된 게 없으면 현재 폴더 경로 반환
                        var folder = doc.GetType().InvokeMember(
                            "Folder",
                            System.Reflection.BindingFlags.GetProperty,
                            null, doc, null);
                        var self = folder.GetType().InvokeMember(
                            "Self",
                            System.Reflection.BindingFlags.GetProperty,
                            null, folder, null);
                        string? folderPath = self.GetType().InvokeMember(
                            "Path",
                            System.Reflection.BindingFlags.GetProperty,
                            null, self, null) as string;

                        return folderPath;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        public static async Task<(string? Url, string? Title)> GetActiveBrowserInfoAsync(int timeoutMs = 1000)
        {
            string? url = null;
            string? title = null;

            try
            {
                return await Task.Run(() =>
                {
                    IntPtr hwnd = GetForegroundWindow();

                    // 1. 윈도우 Title 가져오기
                    var titleBuilder = new StringBuilder(256);
                    _ = GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
                    var splitted = titleBuilder.ToString().Split();

                    title = splitted.FirstOrDefault();
                    if (splitted.Length >= 2) title += " " + splitted[1];

                    // 2. URL 가져오기 (주소창)
                    var element = AutomationElement.FromHandle(hwnd);
                    if (element != null)
                    {
                        // 주소창 Edit 컨트롤 찾기
                        var urlBar = element.FindFirst(TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                        if (urlBar != null && urlBar.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                        {
                            url = ((ValuePattern)pattern).Current.Value;
                            if (string.IsNullOrWhiteSpace(url))
                                url = null;
                        }
                    }
                    return (url, title);
                }).WithTimeout(timeoutMs);
            }
            catch (TimeoutException tex)
            {
                Logger.Error(tex);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return (url, title);
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int millisecondsTimeout)
        {
            using var timeoutCts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsTimeout, timeoutCts.Token));
            if (completedTask == task)
            {
                timeoutCts.Cancel();
                return await task;
            }
            else throw new TimeoutException("Failed to parse browser information.");
        }
    }
}
