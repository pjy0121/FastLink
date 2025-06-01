using NHotkey;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using FastLink.Models;
using FastLink.Services;
using FastLink.Utils;
using MahApps.Metro.Controls;
using FormsScreen = System.Windows.Forms.Screen;

namespace FastLink
{
    public partial class MainWindow : MetroWindow, GongSolutions.Wpf.DragDrop.IDropTarget, INotifyPropertyChanged
    {
        private const int BrowerParsingTimeout = 3000;
        public ObservableCollection<RowItem> RowItems { get; set; } = [];
        private TrayService _trayService;
        private readonly HotkeyService _hotkeyService = new();
        private bool _isExitByTray = false;
        private readonly AppSettings appSettings;
        private bool _isFileLoading = false;
        private readonly List<HotkeyInfo> _specialHotkeys;

        public bool IsAutoStart
        {
            get => appSettings.AutoStart;
            set
            {
                if (appSettings.AutoStart != value)
                {
                    appSettings.AutoStart = value;
                    OnPropertyChanged(nameof(IsAutoStart));
                    AutoStartService.Set(value);
                    _trayService?.SetAutoStartChecked(value);
                    SettingsService.Save(appSettings);
                }
            }
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    OnPropertyChanged(nameof(SearchKeyword));
                    CollectionViewSource.GetDefaultView(RowItems).Refresh();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += CommonEvents.Window_PreviewKeyDown;
            AddHotkeyKeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;
            QuickViewHotkeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;

            appSettings = SettingsService.Load();
            DataContext = this;

            _specialHotkeys =
            [
                new ()
                {
                    Key = Enum.TryParse<Key>(appSettings.AddHotkey, out var addHotkey) ? addHotkey : Key.A,
                    Handlers =
                    {
                        new HandlerWithTag
                        {
                            Handler = OnAddRowHotkeyPressed,
                            Tag = null
                        }
                    }
                },
                new()
                {
                    Key = Enum.TryParse<Key>(appSettings.QuickViewHotkey, out var quickViewHotkey) ? quickViewHotkey : Key.Q,
                    Handlers =
                    {
                        new HandlerWithTag
                        {
                            Handler = OnQuickViewHotkeyPressed,
                            Tag = null
                        }
                    }
                }
            ];

            // Load data
            LoadFastLinkFile(appSettings.SaveFilePath);

            // Search filter
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(RowItems);
            view.Filter = DataGridFilter;

            // Tray icon
            _trayService = new TrayService(IsAutoStart)
            {
                OnAddRow = () => System.Windows.Application.Current.Dispatcher.Invoke(() => ShowAddRowWindowOnForegroundMonitor("", "", RowType.File)),
                OnAutoStartChanged = (val) => System.Windows.Application.Current.Dispatcher.Invoke(() => IsAutoStart = val),
                OnExit = () =>
                {
                    _isExitByTray = true;
                    _trayService.Dispose();
                    System.Windows.Application.Current.Shutdown();
                }
            };

            // Load settings to UI
            SetUIFromSettings();
            RowItems.CollectionChanged += RowItems_CollectionChanged;

            if (IsAutoStart)    // 자동 시작 시 트레이만 보이도록 설정
                Hide();
        }

        private void RowItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isFileLoading) return;

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
                    RefreshAllHotkeys();
                    break;
            }
            SaveRows();
        }

        private void OnRowAdded(RowItem row)
        {
            _hotkeyService.RegisterRowHotkey(row);
        }

        private void OnRowDeleted(RowItem row)
        {
            _hotkeyService.RemoveRowHotkey(row);
        }

        private void SetUIFromSettings()
        {
            // Modifier
            AddCtrlCheck.IsChecked = appSettings.BaseModifier.Contains("Control");
            AddShiftCheck.IsChecked = appSettings.BaseModifier.Contains("Shift");
            AddAltCheck.IsChecked = appSettings.BaseModifier.Contains("Alt");
            _hotkeyService.BaseModifier = ParseModifiers(appSettings.BaseModifier);

            AddHotkeyKeyBox.Text = appSettings.AddHotkey;
            QuickViewHotkeyBox.Text = appSettings.QuickViewHotkey;
            AutoStartCheck.IsChecked = appSettings.AutoStart;
        }

        private void LoadFastLinkFile(string filePath)
        {
            _isFileLoading = true;

            var loaded = FileService.LoadRows(filePath);
            RowItems.Clear();
            foreach (var item in loaded)
                RowItems.Add(item);

            CollectionViewSource.GetDefaultView(RowItems).Refresh();
            appSettings.SaveFilePath = filePath;
            SettingsService.Save(appSettings);
            RefreshAllHotkeys();

            _isFileLoading = false;
        }

        private static ModifierKeys ParseModifiers(string mod)
        {
            ModifierKeys result = ModifierKeys.None;
            if (mod.Contains("Control")) result |= ModifierKeys.Control;
            if (mod.Contains("Shift")) result |= ModifierKeys.Shift;
            if (mod.Contains("Alt")) result |= ModifierKeys.Alt;
            return result;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isExitByTray)
            {
                e.Cancel = true;
                Hide();
            }
            else _trayService.Dispose();
        }

        private void RefreshAllHotkeys()
        {
            _hotkeyService.RefreshAllHotkeys(RowItems, _specialHotkeys);
        }

        private async void OnAddRowHotkeyPressed(object? sender, HotkeyEventArgs e)
        {
            string? path = ExplorerBrowserService.GetActiveExplorerPath();
            string? name = "";
            RowType type = RowType.File;

            if (!string.IsNullOrWhiteSpace(path))
            {
                if (path.StartsWith("\\\\"))
                    type = RowType.Web;
                else type = RowType.File;
                name = Path.GetFileName(path.TrimEnd('\\'));
            }
            else
            {
                var (url, title) = await ExplorerBrowserService.GetActiveBrowserInfoAsync(BrowerParsingTimeout);
                if (url != null && !url.StartsWith("http"))
                    path = $"http://{url}";
                else path = url;

                if (!string.IsNullOrWhiteSpace(path))
                {
                    name = title;
                    type = RowType.Web;
                }
            }

            System.Windows.Application.Current.Dispatcher.Invoke(
                () => ShowAddRowWindowOnForegroundMonitor(path ?? "", name ?? "", type));
            e.Handled = true;
        }

        private void OnQuickViewHotkeyPressed(object? sender, HotkeyEventArgs e)
        {
            var quickView = new QuickViewWindow(RowItems);
            var desktop = SystemParameters.WorkArea;
            quickView.Left = desktop.Right - quickView.Width - 16;
            quickView.Top = desktop.Bottom - quickView.Height - 16;
            quickView.Show();

            e.Handled = true;
        }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is RowItem row)
                CommonUtils.CopyRowPath(row);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is RowItem rowItem)
            {
                var editWindow = new AddRowWindow
                {
                    Owner = this
                };

                editWindow.NameBox.Text = rowItem.Name;
                editWindow.PathBox.Text = rowItem.Path;
                editWindow.TypeCombo.SelectedIndex = (rowItem.Type == RowType.File) ? 0 : 1;
                editWindow.HotkeyKeyBox.Text = rowItem.HotkeyKey;

                if (editWindow.ShowDialog() == true)
                {
                    rowItem.Name = editWindow.ItemName;
                    rowItem.Path = editWindow.ItemPath;
                    rowItem.Type = editWindow.ItemType;
                    rowItem.HotkeyKey = editWindow.ItemHotkeyKey;

                    RefreshAllHotkeys();    // Hotkey가 바뀔 수 있으므로 초기화
                    SaveRows();
                    CollectionViewSource.GetDefaultView(DataGrid.ItemsSource).Refresh();
                }
            }
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
                var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);
                if (view != null && view.CanSort)
                    view.SortDescriptions.Clear();

                // 모든 컬럼의 정렬 표식 제거
                foreach (var column in DataGrid.Columns)
                    column.SortDirection = null;

                e.Handled = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddRowWindowOnForegroundMonitor("", "", RowType.File);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.Tag is RowItem row)
            {
                var result = System.Windows.MessageBox.Show("Are you sure you want to delete?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    RowItems.Remove(row);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "FastLink file (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select file to load"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    LoadFastLinkFile(dlg.FileName);
                    SaveRows();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to load input file.\n\n" + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveRows()
        {
            try
            {
                FileService.SaveRows(appSettings.SaveFilePath, RowItems);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save input file.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void ApplyModifiers_Click(object sender, RoutedEventArgs e)
        {
            var modifier =
                (AddCtrlCheck.IsChecked == true ? "Control," : "") +
                (AddShiftCheck.IsChecked == true ? "Shift," : "") +
                (AddAltCheck.IsChecked == true ? "Alt," : "");
            modifier = modifier.TrimEnd(',');

            appSettings.BaseModifier = modifier;
            _hotkeyService.BaseModifier = ParseModifiers(modifier);

            SettingsService.Save(appSettings);
            RefreshAllHotkeys();
            System.Windows.MessageBox.Show("Base modifiers has been changed.");
        }

        private void ApplyAddHotkey_Click(object sender, RoutedEventArgs e)
        {
            _specialHotkeys[0].Key = Enum.TryParse<Key>(AddHotkeyKeyBox.Text.Trim(), out var k) ? k : Key.A;
            appSettings.AddHotkey = AddHotkeyKeyBox.Text.Trim();

            SettingsService.Save(appSettings);
            RefreshAllHotkeys();
            System.Windows.MessageBox.Show("Add hotkey has been changed.");
        }

        private void QuickViewHotkeyApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _specialHotkeys[1].Key = Enum.TryParse<Key>(QuickViewHotkeyBox.Text.Trim(), out var k) ? k : Key.Q;
            appSettings.QuickViewHotkey = QuickViewHotkeyBox.Text.Trim();

            SettingsService.Save(appSettings);
            RefreshAllHotkeys();
            System.Windows.MessageBox.Show("QuickView hotkey has been changed.");
        }

        private void AutoStartCheck_Checked(object sender, RoutedEventArgs e)
        {
            IsAutoStart = true;
        }

        private void AutoStartCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAutoStart = false;
        }

        private void ShowAddRowWindowOnForegroundMonitor(string path, string name, RowType type)
        {
            var (centerX, centerY) = ExplorerBrowserService.GetForegroundWindowCenter();
            var screen = FormsScreen.PrimaryScreen;
            foreach (var scr in FormsScreen.AllScreens)
            {
                if (centerX >= scr.Bounds.Left && centerX < scr.Bounds.Right &&
                    centerY >= scr.Bounds.Top && centerY < scr.Bounds.Bottom)
                {
                    screen = scr;
                    break;
                }
            }

            var addWindow = new AddRowWindow(path, type);
            addWindow.NameBox.Text = name;
            addWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            addWindow.Topmost = true;
            addWindow.Left = screen.Bounds.Left + (screen.Bounds.Width - addWindow.Width) / 2;
            addWindow.Top = screen.Bounds.Top + (screen.Bounds.Height - addWindow.Height) / 2;
            addWindow.ShowInTaskbar = false;
            addWindow.ShowActivated = true;
            addWindow.Activate();

            if (addWindow.ShowDialog() == true)
            {
                RowItems.Add(new RowItem
                {
                    Name = addWindow.ItemName,
                    Path = addWindow.ItemPath,
                    Type = addWindow.ItemType,
                    HotkeyKey = addWindow.ItemHotkeyKey
                });
                CollectionViewSource.GetDefaultView(RowItems).Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // GongSolutions.Wpf.DragDrop IDropTarget
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
    }
}
