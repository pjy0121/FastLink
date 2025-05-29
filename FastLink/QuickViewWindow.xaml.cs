using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink
{
    public partial class QuickViewWindow : MahApps.Metro.Controls.MetroWindow
    {
        public QuickViewWindow(ObservableCollection<RowItem> items)
        {
            InitializeComponent();
            PreviewKeyDown += CommonEvents.Window_PreviewKeyDown;

            LinkDataGrid.ItemsSource = items;
        }

        private void LinkDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is RowItem item)
            {
                var view = CollectionViewSource.GetDefaultView(LinkDataGrid.ItemsSource) as CollectionView;
                int index = view?.IndexOf(item) ?? -1;
                item.RowNumber = index + 1;
            }
        }

        private void LinkDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LinkDataGrid.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                Hide();
            }
        }

        private void LinkDataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && LinkDataGrid.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                e.Handled = true;
                Hide();
            }
        }

        private void LinkDataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = e.OriginalSource as DependencyObject;
            while (depObj != null && depObj is not DataGridRow && depObj is not DataGridColumnHeader)
                depObj = VisualTreeHelper.GetParent(depObj);

            // Row 우클릭 시 path copy 후 숨김
            if (depObj is DataGridRow row && row.Item is RowItem data)
            {
                CommonUtils.CopyRowPath(data);
                e.Handled = true;
                Hide();
                return;
            }

            // Header 우클릭 시 정렬 해제
            if (depObj is DataGridColumnHeader)
            {
                var view = CollectionViewSource.GetDefaultView(LinkDataGrid.ItemsSource);
                if (view != null && view.CanSort)
                    view.SortDescriptions.Clear();

                // 모든 컬럼의 정렬 표식 제거
                foreach (var column in LinkDataGrid.Columns)
                    column.SortDirection = null;

                e.Handled = true;
            }
        }
    }
}
