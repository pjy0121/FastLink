using System.Windows;
using System.Windows.Input;
using FastLink.Models;

namespace FastLink
{
    public partial class AddRowWindow : MahApps.Metro.Controls.MetroWindow
    {
        public string ItemName => NameBox.Text.Trim();
        public string ItemPath => PathBox.Text.Trim();
        public RowType ItemType => TypeCombo.SelectedIndex == 1 ? RowType.Web : RowType.File;
        public string ItemHotkeyKey => HotkeyKeyBox.Text.Trim().ToUpper();

        public AddRowWindow()
        {
            InitializeComponent();
            TypeCombo.SelectedIndex = 0;
        }

        public AddRowWindow(string path, RowType type) : this()
        {
            PathBox.Text = path ?? "";
            PathBox.CaretIndex = PathBox.Text.Length;   // Text의 뒷 부분이 보이도록 설정
            PathBox.Focus();

            TypeCombo.SelectedIndex = (type == RowType.Web) ? 1 : 0;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemPath))
            {
                System.Windows.MessageBox.Show("Please enter a path.");
                return;
            }
            DialogResult = true;
        }

        private void HotkeyKeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                e.Handled = true;
                return;
            }
            if (e.Key >= Key.F1 && e.Key <= Key.F24)
                HotkeyKeyBox.Text = e.Key.ToString();
            else if (e.Key >= Key.A && e.Key <= Key.Z)
                HotkeyKeyBox.Text = e.Key.ToString();
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
                HotkeyKeyBox.Text = e.Key.ToString().Substring(1);
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                HotkeyKeyBox.Text = "Num" + (e.Key - Key.NumPad0);
            else
                HotkeyKeyBox.Text = e.Key.ToString();

            HotkeyKeyBox.CaretIndex = HotkeyKeyBox.Text.Length;
            e.Handled = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
