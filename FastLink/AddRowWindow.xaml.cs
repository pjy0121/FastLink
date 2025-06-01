using System.Windows;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink
{
    public partial class AddRowWindow : MahApps.Metro.Controls.MetroWindow
    {
        public string ItemName => NameBox.Text.Trim();
        public string ItemPath => PathBox.Text.Trim();
        public RowType ItemType => TypeCombo.SelectedIndex == 0 ? RowType.File : RowType.Web;
        public string ItemHotkeyKey => HotkeyKeyBox.Text.Trim().ToUpper();

        public AddRowWindow()
        {
            InitializeComponent();
            PreviewKeyDown += CommonEvents.Window_PreviewKeyDown;
            HotkeyKeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;

            TypeCombo.SelectedIndex = 0;
        }

        public AddRowWindow(string path, RowType type) : this()
        {
            PathBox.Text = path ?? "";
            PathBox.CaretIndex = PathBox.Text.Length;   // Text의 뒷 부분이 보이도록 설정
            PathBox.Focus();

            TypeCombo.SelectedIndex = (type == RowType.File) ? 0 : 1;
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
