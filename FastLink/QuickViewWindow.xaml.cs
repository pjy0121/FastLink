using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink
{
    public partial class QuickViewWindow : MahApps.Metro.Controls.MetroWindow
    {
        private CollectionViewSource _viewSource;

        public QuickViewWindow(ObservableCollection<RowItem> items)
        {
            InitializeComponent();
            PreviewKeyDown += CommonEvents.Window_PreviewKeyDown;

            // Search filter
            _viewSource = new CollectionViewSource { Source = items };
            _viewSource.Filter += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                    e.Accepted = true;
                else
                {
                    var row = e.Item as RowItem;
                    var keyword = SearchBox.Text.Trim().ToLower();
                    e.Accepted =
                        (row.Name?.ToLower().Contains(keyword) ?? false)
                        || (row.Path?.ToLower().Contains(keyword) ?? false)
                        || (row.Type.ToString().ToLower().Contains(keyword))
                        || (row.HotkeyKey?.ToLower().Contains(keyword) ?? false);
                }
            };

            DataGrid.ItemsSource = _viewSource.View;
            items.CollectionChanged += (s, e) => _viewSource.View.Refresh();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewSource.View.Refresh();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is RowItem item)
            {
                var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource) as CollectionView;
                int index = view?.IndexOf(item) ?? -1;
                item.RowNumber = index + 1;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGrid.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                Close();
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataGrid.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                Close();
            }
        }

        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = e.OriginalSource as DependencyObject;
            while (depObj != null && depObj is not DataGridRow && depObj is not DataGridColumnHeader)
                depObj = VisualTreeHelper.GetParent(depObj);

            // Row 우클릭 시 path copy 후 숨김
            if (depObj is DataGridRow row && row.Item is RowItem data)
            {
                CommonUtils.CopyRowPath(data);
                Close();
                return;
            }

            // Header 우클릭 시 정렬 해제
            if (depObj is DataGridColumnHeader)
            {
                var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);
                if (view != null && view.CanSort)
                    view.SortDescriptions.Clear();

                // 모든 컬럼의 정렬 표식 제거
                foreach (var column in DataGrid.Columns)
                    column.SortDirection = null;

                e.Handled = true;
            }
        }
    }
}
