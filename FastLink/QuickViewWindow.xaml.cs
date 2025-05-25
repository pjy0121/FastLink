using System.Collections.ObjectModel;
using System.Windows.Input;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink
{
    public partial class QuickViewWindow : MahApps.Metro.Controls.MetroWindow
    {
        public QuickViewWindow(ObservableCollection<RowItem> items)
        {
            InitializeComponent();
            LinkListView.ItemsSource = items;
        }

        private void LinkListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LinkListView.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                Close();
            }
        }

        private void LinkListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is RowItem data)
            {
                CommonUtils.CopyRowPath(data);
                Close();
            }
        }
    }
}
