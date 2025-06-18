using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FastLink.Models;
using FastLink.Services;
using FastLink.Utils;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;

namespace FastLink
{
    public partial class ClipboardWindow : INotifyPropertyChanged, GongSolutions.Wpf.DragDrop.IDropTarget
    {
        private bool _isFileLoading = false;

        public ObservableCollection<RowItem> RowItems { get; set; } = [];

        public ClipboardWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadFastLinkFile(SettingsService.Settings.ClipboardFilePath);

            // DataGrid 필터링
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(RowItems);
            view.Filter = DataGridFilter;

            RowItems.CollectionChanged += RowItems_CollectionChanged;
        }

        private void UpdateRowNumbers()
        {
            for (int i = 0; i < RowItems.Count; i++)
                RowItems[i].RowNumber = i + 1;  // 1부터 시작
        }

        private void LoadFastLinkFile(string filePath)
        {
            _isFileLoading = true;

            ClearHotkeys();
            RowItems.Clear();

            var rows = FileService.LoadRows<RowItem>(filePath);
            foreach (var row in rows)
                RowItems.Add(row);

            UpdateRowNumbers();
            RegisterHotkeys();

            RefreshView();
            SettingsService.Settings.ClipboardFilePath = filePath;

            _isFileLoading = false;
        }

        public void ClearHotkeys()
        {
            foreach (var row in RowItems)
                HotkeyService.RemoveRowHotkey(row);
        }

        public void RegisterHotkeys()
        {
            foreach (var row in RowItems)
                HotkeyService.RegisterRowHotkey(row);
        }

        public void RefreshView()
        {
            CollectionViewSource.GetDefaultView(RowItems).Refresh();
        }

        private void SaveRows()
        {
            try
            {
                FileService.SaveRows(SettingsService.Settings.ClipboardFilePath, RowItems);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save input file.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddRow(string name, string path, string hotkeyKey, RowType rowType, object? preview)
        {
            if (preview == null) return;
            switch (rowType)
            {
                case RowType.Text:
                case RowType.Html:
                    if (preview is string text)
                    {
                        var item = new ClipboardItem<string>
                        {
                            Type = rowType,
                            Name = name,
                            HotkeyKey = hotkeyKey,
                            Data = text
                        };
                        RowItems.Add(item);
                    }
                    break;
                case RowType.FileList:
                    if (preview is List<string> files)
                    {
                        var item = new ClipboardItem<List<string>>
                        {
                            Type = rowType,
                            Name = name,
                            HotkeyKey = hotkeyKey,
                            Data = files
                        };
                        RowItems.Add(item);
                    }
                    break;
                case RowType.Image:
                    if (preview is byte[] bytes)
                    {
                        var item = new ClipboardItem<byte[]>
                        {
                            Type = rowType,
                            Name = name,
                            Path = path,    // 원본 파일 경로
                            HotkeyKey = hotkeyKey,
                            Data = bytes  // preview만 data에 저장
                        };
                        RowItems.Add(item);
                    }
                    break;
            }
        }

        private void RowItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isFileLoading) return;

            UpdateRowNumbers();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (RowItem newRow in e.NewItems!)
                        OnRowAdded(newRow);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (RowItem oldRow in e.OldItems!)
                        OnRowDeleted(oldRow);
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (RowItem newRow in e.NewItems!)
                        OnRowMoved(newRow);
                    break;
            }
            RefreshView();
            SaveRows();
        }

        private static void OnRowAdded(RowItem row)
        {
            HotkeyService.RegisterRowHotkey(row);
        }

        private static void OnRowDeleted(RowItem row)
        {
            HotkeyService.RemoveRowHotkey(row);

            if (row.Type == RowType.Image && !string.IsNullOrEmpty(row.Path))
            {
                try
                {
                    if (File.Exists(row.Path))
                        File.Delete(row.Path);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex); // 예외 로깅
                }
            }
        }

        private static void OnRowMoved(RowItem row)
        {
            // 삭제했다가 다시 등록 => 바뀐 index 기준으로 재정렬됨
            HotkeyService.RemoveRowHotkey(row);
            HotkeyService.RegisterRowHotkey(row);
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
            RefreshView();
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
                switch (row.Type)
                {
                    case RowType.Text:
                    case RowType.Web:
                        if (row is ClipboardItem<string> textItem)
                            Clipboard.SetText(textItem.Data ?? string.Empty);
                        break;
                    case RowType.Image:
                        if (row is ClipboardItem<byte[]> imageItem && !string.IsNullOrEmpty(imageItem.Path)
                            && File.Exists(imageItem.Path))
                        {
                            var image = new BitmapImage(new Uri(imageItem.Path));
                            Clipboard.SetImage(image);
                        }
                        break;
                    case RowType.Html:
                        if (row is ClipboardItem<string> htmlItem)
                        {
                            var dataObj = new System.Windows.DataObject();
                            dataObj.SetData(DataFormats.Html, htmlItem.Data);
                            Clipboard.SetDataObject(dataObj);
                        }
                        break;
                    case RowType.FileList:
                        if (row is ClipboardItem<List<string>> fileItem)
                        {
                            var files = new StringCollection();
                            files.AddRange([.. fileItem.Data]);
                            Clipboard.SetFileDropList(files);
                        }
                        break;
                }
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
                RefreshView();
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

                    string oldHotkeyKey = row.HotkeyKey;
                    row.HotkeyKey = editRowWindow.InputHotkey;

                    HotkeyService.MoveHandlerToNewHotkey(row, oldHotkeyKey);
                    SaveRows();
                    RefreshView();
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
                    LoadFastLinkFile(dialog.FileName);
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
