using System.IO;
using System.Windows.Input;
using FastLink.Models;

namespace FastLink.Utils
{
    public static class CommonEvents
    {
        public static void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (sender is System.Windows.Window window)
                    window.Hide();
            }
        }
    }
}