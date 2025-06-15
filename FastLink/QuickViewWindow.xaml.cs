using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using FastLink.Models;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using DataObject = System.Windows.DataObject;
using DataFormats = System.Windows.DataFormats;
using FastLink.Utils;

namespace FastLink
{
    public partial class QuickViewWindow : MahApps.Metro.Controls.MetroWindow
    {
        private static Tab _selectedTab = Tab.Link;     // 항상 마지막에 선택된 tab 저장

        private readonly ObservableCollection<RowItem> _linkItems;
        private readonly ObservableCollection<RowItem> _clipboardItems;

        public QuickViewWindow(ObservableCollection<RowItem> linkItems, ObservableCollection<RowItem> clipboardItems)
        {
            InitializeComponent();
            _linkItems = linkItems;
            _clipboardItems = clipboardItems;
            SelectTab(_selectedTab);

            CollectionView viewLink = (CollectionView)CollectionViewSource.GetDefaultView(_linkItems);
            viewLink.Filter = LinkFilter;

            CollectionView viewClipboard = (CollectionView)CollectionViewSource.GetDefaultView(_clipboardItems);
            viewClipboard.Filter = ClipboardFilter;

            Loaded += (s, e) => SearchBox.Focus();
        }

        private bool LinkFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            string search = SearchBox.Text.Trim();
            if (item is not RowItem row) return false;
            return
                (row.Name?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (row.Path?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (row.HotkeyKey?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (row.Type.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        private bool ClipboardFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            string search = SearchBox.Text.Trim();
            if (item is not RowItem row) return false;
            return
                (row.Name?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (row.Path?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (row.HotkeyKey?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (row.Type.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        private void FocusFirstCellInGrid()
        {
            if (MainDataGrid.Items.Count == 0) return;
            MainDataGrid.SelectedIndex = 0;
            MainDataGrid.CurrentCell = new DataGridCellInfo(MainDataGrid.Items[0], MainDataGrid.Columns[0]);
            MainDataGrid.UpdateLayout();
            MainDataGrid.ScrollIntoView(MainDataGrid.Items[0]);

            var row = (DataGridRow)MainDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
            if (row != null)
            {
                var presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    row.ApplyTemplate();
                    presenter = FindVisualChild<DataGridCellsPresenter>(row);
                }
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0);
                cell?.Focus();
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                    return t;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void SelectTab(Tab tab)
        {
            _selectedTab = tab;
            if (tab == Tab.Link)
            {
                MainDataGrid.ItemsSource = _linkItems;
                PreviewColumn.Visibility = Visibility.Collapsed;
                LblLinkTab.Style = (Style)FindResource("IndexTabLabelSelectedStyle");
                LblClipboardTab.Style = (Style)FindResource("IndexTabLabelStyle");
            }
            else
            {
                MainDataGrid.ItemsSource = _clipboardItems;
                PreviewColumn.Visibility = Visibility.Visible;
                LblLinkTab.Style = (Style)FindResource("IndexTabLabelStyle");
                LblClipboardTab.Style = (Style)FindResource("IndexTabLabelSelectedStyle");
            }

            CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource)?.Refresh();

            // 탭 전환 이후 SearchBox에 포커스
            Dispatcher.BeginInvoke(new Action(() => SearchBox.Focus()), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void LblLinkTab_MouseDown(object sender, MouseButtonEventArgs e) => SelectTab(Tab.Link);
        private void LblClipboardTab_MouseDown(object sender, MouseButtonEventArgs e) => SelectTab(Tab.Clipboard);

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource)?.Refresh();
        }

        private void MainDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is RowItem item)
            {
                var view = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource) as ListCollectionView;
                int index = view?.IndexOf(item) ?? -1;
                item.RowNumber = index + 1;
            }
        }

        private void MainDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                SelectTab(_selectedTab == Tab.Link ? Tab.Clipboard : Tab.Link);
            }
            else if (e.Key == Key.Enter)
            {
                HandleRowAction();
                e.Handled = true;
            }
        }

        private void MainDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HandleRowAction();
        }

        private async void MainDataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = e.OriginalSource as DependencyObject;
            while (depObj != null && depObj is not DataGridRow)
                depObj = VisualTreeHelper.GetParent(depObj);

            if (depObj is DataGridRow gridRow && gridRow.Item is RowItem row)
            {
                if (row.Type == RowType.File || row.Type == RowType.Web)
                {
                    Clipboard.SetText(row.Path ?? string.Empty);    // copy path to clipboard

                    e.Handled = true;
                    await Task.Delay(200);
                }
                else
                {
                    CommonUtils.OpenRowPath(row);  // open original file if path exists
                    e.Handled = true;
                }
                Close();
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                SelectTab(_selectedTab == Tab.Link ? Tab.Clipboard : Tab.Link);
            }

            // 방향키 입력 시 DataGrid로 포커스 이동
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = true;
                FocusFirstCellInGrid();
            }
        }

        private void HandleRowAction()
        {
            if (MainDataGrid.SelectedItem is not RowItem row)
                return;

            switch (row.Type)
            {
                case RowType.File:
                case RowType.Web:
                    CommonUtils.OpenRowPath(row);
                    break;
                case RowType.Text:
                    if (row is ClipboardItem<string> textItem)
                        Clipboard.SetText(textItem.Data ?? string.Empty);
                    break;
                case RowType.Image:
                    if (row is ClipboardItem<byte[]> imageItem && !string.IsNullOrEmpty(imageItem.Path)
                        && System.IO.File.Exists(imageItem.Path))
                    {
                        var image = new BitmapImage(new Uri(imageItem.Path));
                        Clipboard.SetImage(image);
                    }
                    break;
                case RowType.Html:
                    if (row is ClipboardItem<string> htmlItem)
                    {
                        var dataObj = new DataObject();
                        dataObj.SetData(DataFormats.Html, htmlItem.Data);
                        Clipboard.SetDataObject(dataObj);
                    }
                    break;
                case RowType.FileList:
                    if (row is ClipboardItem<System.Collections.Generic.List<string>> fileItem)
                    {
                        var files = new System.Collections.Specialized.StringCollection();
                        files.AddRange([.. fileItem.Data]);
                        Clipboard.SetFileDropList(files);
                    }
                    break;
            }
            Close();
        }
    }
}
