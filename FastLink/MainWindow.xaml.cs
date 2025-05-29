using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using FastLink.Models;
using FastLink.Services;
using FastLink.Utils;
using FormsScreen = System.Windows.Forms.Screen;

namespace FastLink
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow, GongSolutions.Wpf.DragDrop.IDropTarget, INotifyPropertyChanged
    {
        private const int BrowerParsingTimeout = 3000;

        public ObservableCollection<RowItem> RowItems { get; set; } = [];
        private TrayService _trayService;
        private HotkeyService _hotKeyService = new();
        private bool _isExitByTray = false;
        private AppSettings appSettings;
        private bool _isFileLoading = false;

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

        // Hotkeys
        private Key _addHotkey;
        private Key _quickViewHotkey;

        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += CommonEvents.Window_PreviewKeyDown;
            AddHotkeyKeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;
            QuickViewHotkeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;

            appSettings = SettingsService.Load();
            DataContext = this;

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

            RegisterAddHotkey();
            RegisterQuickViewHotkey();
            RegisterRowHotkeys();

            RowItems.CollectionChanged += (s, e) =>
            {
                if (!_isFileLoading)
                {
                    RegisterRowHotkeys();
                    SaveRows();
                    CollectionViewSource.GetDefaultView(RowItems).Refresh();
                }
            };

            if (IsAutoStart)    // 자동 시작 시 트레이만 보이도록 설정
                Hide();
        }

        private void SetUIFromSettings()
        {
            // Modifier
            AddCtrlCheck.IsChecked = appSettings.BaseModifier.Contains("Control");
            AddShiftCheck.IsChecked = appSettings.BaseModifier.Contains("Shift");
            AddAltCheck.IsChecked = appSettings.BaseModifier.Contains("Alt");
            _hotKeyService.BaseModifier = ParseModifiers(appSettings.BaseModifier);

            AddHotkeyKeyBox.Text = appSettings.AddHotkey;
            QuickViewHotkeyBox.Text = appSettings.QuickViewHotkey;
            AutoStartCheck.IsChecked = appSettings.AutoStart;

            _addHotkey = Enum.TryParse<Key>(appSettings.AddHotkey, out var k1) ? k1 : Key.None;
            _quickViewHotkey = Enum.TryParse<Key>(appSettings.QuickViewHotkey, out var k2) ? k2 : Key.None;
        }

        private void SaveSettingsFromUI()
        {
            var modifier =
                (AddCtrlCheck.IsChecked == true ? "Control," : "") +
                (AddShiftCheck.IsChecked == true ? "Shift," : "") +
                (AddAltCheck.IsChecked == true ? "Alt," : "");
            appSettings.BaseModifier = modifier.TrimEnd(',');
            _hotKeyService.BaseModifier = ParseModifiers(appSettings.BaseModifier);

            appSettings.AddHotkey = AddHotkeyKeyBox.Text.Trim();
            appSettings.QuickViewHotkey = QuickViewHotkeyBox.Text.Trim();
            appSettings.AutoStart = AutoStartCheck.IsChecked == true;
            SettingsService.Save(appSettings);
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

            _isFileLoading = false;
        }

        private ModifierKeys ParseModifiers(string mod)
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

        private void RegisterAddHotkey()
        {
            _hotKeyService.RegisterHotkey(
                "AddRowHotkey", _addHotkey,
                AddRowHotkeyHandler
            );
        }

        private void RegisterQuickViewHotkey()
        {
            _hotKeyService.RegisterHotkey(
                "QuickViewHotkey", _quickViewHotkey,
                QuickViewHotkeyHandler
            );
        }

        private void RegisterRowHotkeys()
        {
            _hotKeyService.RegisterRowHotkeys(RowItems, RowHotkeyHandler);
        }

        private void QuickViewHotkeyHandler(object sender, NHotkey.HotkeyEventArgs e)
        {
            var quickView = new QuickViewWindow(RowItems);
            var desktop = SystemParameters.WorkArea;
            quickView.Left = desktop.Right - quickView.Width - 16;
            quickView.Top = desktop.Bottom - quickView.Height - 16;
            quickView.Show();

            e.Handled = true;
        }

        private async void AddRowHotkeyHandler(object sender, NHotkey.HotkeyEventArgs e)
        {
            string? path = ExplorerBrowserService.GetActiveExplorerPath();
            string? name = "";
            RowType type = RowType.File;

            if (!string.IsNullOrWhiteSpace(path))
            {
                name = Path.GetFileName(path.TrimEnd('\\'));
                type = RowType.File;
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

        private void RowHotkeyHandler(object sender, NHotkey.HotkeyEventArgs e)
        {
            string keyStr = e.Name.Replace("RowHotkey_", "");
            var row = _hotKeyService.GetRowByKey(keyStr);
            if (row != null)
            {
                CommonUtils.OpenRowPath(row);
                e.Handled = true;
            }
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
                var result = System.Windows.MessageBox.Show("Are you sure you want to delete?", "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                if (result == System.Windows.MessageBoxResult.Yes)
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
                    SaveSettingsFromUI();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to load input file.\n\n" + ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show("Failed to save input file.\n\n" + ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

        private void ApplyAddHotkey_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUI();
            _addHotkey = Enum.TryParse<Key>(AddHotkeyKeyBox.Text.Trim(), out var k) ? k : Key.F1;
            RegisterAddHotkey();
            System.Windows.MessageBox.Show("Add hotkey has been changed.");
        }

        private void QuickViewHotkeyApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUI();
            _quickViewHotkey = Enum.TryParse<Key>(QuickViewHotkeyBox.Text.Trim(), out var k) ? k : Key.Space;
            RegisterQuickViewHotkey();
            System.Windows.MessageBox.Show("QuickView hotkey has been changed.");
        }

        private void AutoStartCheck_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUI();
        }

        private void AutoStartCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUI();
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
