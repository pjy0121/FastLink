using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
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

        public static byte[] BitmapSourceToBytes(BitmapSource image)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

        public static BitmapSource BytesToBitmapSource(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public static BitmapSource CreateThumbnail(BitmapSource source, int width, int height)
        {
            double scaleX = width / (double)source.PixelWidth;
            double scaleY = height / (double)source.PixelHeight;
            var scale = Math.Min(scaleX, scaleY);
            var transform = new System.Windows.Media.ScaleTransform(scale, scale);
            var scaled = new TransformedBitmap(source, transform);
            return scaled;
        }

        public static string SavePng(BitmapSource image)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FastLink", "Images");
            Directory.CreateDirectory(dir);
            var filename = $"{Guid.NewGuid()}.png";
            var path = Path.Combine(dir, filename);
            using var fs = new FileStream(path, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(fs);

            return path;
        }

        public static bool IsUrl(string text)
        {
            return Regex.IsMatch(text, @"^(https?|ftp)://[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase);
        }
    }
}
