using System.Windows;
using System.Windows.Input;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink
{
    public partial class AddRowWindow : MahApps.Metro.Controls.MetroWindow
    {
        public string InputName => NameBox.Text.Trim();
        public string InputPath => PathBox.Text.Trim();
        public RowType InputType => TypeCombo.SelectedIndex == 0 ? RowType.File : RowType.Web;
        public string InputHotkey => HotkeyKeyBox.Text.Trim().ToUpper();

        public AddRowWindow(string title)
        {
            InitializeComponent();
            Title = title;
            HotkeyKeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;

            TypeCombo.SelectedIndex = 0;
        }

        public void SetFields(string? name, string? path, string? hotkey, RowType type)
        {
            NameBox.Text = name ?? string.Empty;
            PathBox.Text = path ?? string.Empty;
            PathBox.CaretIndex = PathBox.Text.Length;   // Text의 뒷 부분이 보이도록 설정
            PathBox.Focus();
            HotkeyKeyBox.Text = hotkey ?? string.Empty;

            TypeCombo.SelectedIndex = (type == RowType.File) ? 0 : 1;
        }

        public void AddRowWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
            if (string.IsNullOrWhiteSpace(InputPath))
            {
                System.Windows.MessageBox.Show("Please enter a path.");
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
