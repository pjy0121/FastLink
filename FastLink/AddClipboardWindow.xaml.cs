using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FastLink.Utils;
using FastLink.Models;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace FastLink
{
    public partial class AddClipboardWindow : MahApps.Metro.Controls.MetroWindow
    {
        const int MaxPreviewWidth = 200;
        const int MinPreviewHeight = 100, MaxPreviewHeight = 200;

        public string InputName => NameBox.Text.Trim();
        public string InputHotkey => HotkeyKeyBox.Text.Trim().ToUpper();
        public object? Preview { get; set; } = null;

        public AddClipboardWindow(string title)
        {
            InitializeComponent();
            Title = title;
            HotkeyKeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;
        }

        private void DynamicTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
                Preview = textBox.Text;     // textBox에 마지막으로 쓰여있는 내용을 저장
        }

        public void SetFields(string? name, string? hotkey, RowType type, object? data)
        {
            NameBox.Text = name ?? string.Empty;
            NameBox.CaretIndex = NameBox.Text.Length;   // Text의 뒷 부분이 보이도록 설정
            NameBox.Focus();
            HotkeyKeyBox.Text = hotkey ?? string.Empty;
            TypeText.Text = type.ToString();
            Preview = data;

            // Show Preview
            object? initialPreview = null;
            switch (type)
            {
                case RowType.Text:
                case RowType.Html:
                    if (data is string text)
                    {
                        var textBox = new System.Windows.Controls.TextBox
                        {
                            Text = text,
                            AcceptsReturn = true,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = System.Windows.Media.Brushes.DarkSlateBlue,
                            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                            MinHeight = MinPreviewHeight,
                            MaxHeight = MaxPreviewHeight
                        };
                        textBox.LostFocus += DynamicTextBox_LostFocus;
                        initialPreview = textBox;
                    }
                    break;
                case RowType.FileList:
                    if (data is List<string> files)
                    {
                        initialPreview = new System.Windows.Controls.ListBox
                        {
                            ItemsSource = files,
                            MaxHeight = MaxPreviewHeight
                        };
                    }
                    break;
                case RowType.Image:
                    if (data is BitmapSource image)
                    {
                        // 썸네일 만들기
                        var thumbnail = CommonUtils.CreateThumbnail(image, MaxPreviewWidth, MaxPreviewHeight);
                        var thumbnailBytes = CommonUtils.BitmapSourceToBytes(thumbnail);
                        Preview = thumbnailBytes;

                        initialPreview = new System.Windows.Controls.Image
                        {
                            Source = thumbnail,
                            MaxWidth = MaxPreviewWidth,
                            MaxHeight = MaxPreviewHeight,
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                    }
                    break;
            }

            initialPreview ??= new TextBlock
            {
                Text = "Current clipboard data type is not supported!",
                Foreground = System.Windows.Media.Brushes.Gray
            };
            PreviewContent.Content = initialPreview;
        }

        public void AddClipboardWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Cancel_Click(CancelButton, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Ok_Click(OkButton, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputName))
            {
                System.Windows.MessageBox.Show("Please enter a name.");
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
