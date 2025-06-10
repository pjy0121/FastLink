using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using FastLink.Models;
using FastLink.Services;
using FastLink.Utils;

namespace FastLink
{
    public partial class ClipboardWindow : INotifyPropertyChanged, GongSolutions.Wpf.DragDrop.IDropTarget
    {
        public ObservableCollection<RowItem> RowItems { get; set; } = new ObservableCollection<RowItem>();

        public ClipboardWindow()
        {
            InitializeComponent();
            DataContext = this;

            // 데이터 로딩 (예: 파일에서)
            LoadRows();

            // DataGrid 필터링
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(RowItems);
            view.Filter = DataGridFilter;

            RowItems.CollectionChanged += RowItems_CollectionChanged;
        }

        private void LoadRows()
        {
            // 예시: 파일에서 JSON 로딩
            var filePath = "FastLink.json";
            if (File.Exists(filePath))
            {
                var loaded = FileService.LoadRows(filePath);
                RowItems.Clear();
                foreach (var item in loaded)
                    RowItems.Add(item);
            }
        }

        private void SaveRows()
        {
            try
            {
                FileService.SaveRows("FastLink.json", RowItems);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save input file.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RowItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SaveRows();
        }

        private bool DataGridFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;
            var row = item as RowItem;
            var keyword = SearchBox.Text.Trim().ToLower();
            return (row.Name?.ToLower().Contains(keyword) ?? false)
                || (row.Path?.ToLower().Contains(keyword) ?? false)
                || (row.Type.ToString().ToLower().Contains(keyword))
                || (row.HotkeyKey?.ToLower().Contains(keyword) ?? false);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(RowItems).Refresh();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is RowItem item)
            {
                var view = CollectionViewSource.GetDefaultView(RowItems) as CollectionView;
                int index = view?.IndexOf(item) ?? -1;
                item.RowNumber = index + 1;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGrid.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                e.Handled = true;
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataGrid.SelectedItem is RowItem row)
            {
                CommonUtils.OpenRowPath(row);
                e.Handled = true;
            }
        }

        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = e.OriginalSource as DependencyObject;
            while (depObj != null && depObj is not DataGridRow && depObj is not DataGridColumnHeader)
                depObj = VisualTreeHelper.GetParent(depObj);

            // Row 우클릭 시 path copy
            if (depObj is DataGridRow row && row.Item is RowItem data)
            {
                CommonUtils.CopyRowPath(data);
                e.Handled = true;
                return;
            }

            // Header 우클릭 시 정렬 해제 및 순서 복원
            if (depObj is DataGridColumnHeader)
            {
                var view = CollectionViewSource.GetDefaultView(RowItems);
                if (view != null && view.CanSort)
                    view.SortDescriptions.Clear();

                foreach (var column in DataGrid.Columns)
                    column.SortDirection = null;

                e.Handled = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addRowWindow = new AddRowWindow("Add Link")
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true,
                ShowInTaskbar = true,
                ShowActivated = true
            };
            addRowWindow.SetFields(null, null, null, RowType.File);

            if (addRowWindow.ShowDialog() == true)
            {
                RowItems.Add(new RowItem
                {
                    Name = addRowWindow.InputName,
                    Path = addRowWindow.InputPath,
                    Type = addRowWindow.InputType,
                    HotkeyKey = addRowWindow.InputHotkey
                });
                CollectionViewSource.GetDefaultView(RowItems).Refresh();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is RowItem row)
            {
                var editRowWindow = new AddRowWindow("Edit Link")
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Topmost = true,
                    ShowInTaskbar = true,
                    ShowActivated = true
                };
                editRowWindow.SetFields(row.Name, row.Path, row.HotkeyKey, row.Type);

                if (editRowWindow.ShowDialog() == true)
                {
                    row.Name = editRowWindow.InputName;
                    row.Path = editRowWindow.InputPath;
                    row.Type = editRowWindow.InputType;
                    row.HotkeyKey = editRowWindow.InputHotkey;
                    SaveRows();
                    CollectionViewSource.GetDefaultView(RowItems).Refresh();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button { Tag: RowItem row })
            {
                var result = System.Windows.MessageBox.Show("Are you sure you want to delete?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    RowItems.Remove(row);
            }
        }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is RowItem row)
                CommonUtils.CopyRowPath(row);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "FastLink file (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select file to load"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var loaded = FileService.LoadRows(dialog.FileName);
                    RowItems.Clear();
                    foreach (var item in loaded)
                        RowItems.Add(item);
                    SaveRows();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to load input file.\n\n" + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void DragOver(GongSolutions.Wpf.DragDrop.IDropInfo dropInfo)
        {
            if (dropInfo.Data is RowItem && dropInfo.TargetCollection is ObservableCollection<RowItem>)
            {
                dropInfo.DropTargetAdorner = GongSolutions.Wpf.DragDrop.DropTargetAdorners.Insert;
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        public void Drop(GongSolutions.Wpf.DragDrop.IDropInfo dropInfo)
        {
            if (dropInfo.Data is RowItem sourceItem && dropInfo.TargetCollection is ObservableCollection<RowItem> collection)
            {
                var oldIndex = collection.IndexOf(sourceItem);
                var newIndex = dropInfo.InsertIndex;
                if (oldIndex != newIndex)
                    collection.Move(oldIndex, newIndex > oldIndex ? newIndex - 1 : newIndex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
