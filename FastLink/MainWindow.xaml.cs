using NHotkey;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FastLink.Models;
using FastLink.Services;
using FastLink.Utils;

namespace FastLink
{
    public partial class MainWindow : MetroWindow, GongSolutions.Wpf.DragDrop.IDropTarget, INotifyPropertyChanged
    {
        private const int BrowerParsingTimeout = 3000;
        private LinkWindow _linkWindow;
        private ClipboardWindow _clipboardWindow;
        private QuickViewWindow? _quickViewWindow;
        private readonly TrayService _trayService;

        private readonly List<SpecialHotkey> _specialHotkeys;
        private bool _isExitByTray = false;

        public bool IsAutoStart
        {
            get => SettingsService.Settings.AutoStart;
            set
            {
                if (SettingsService.Settings.AutoStart != value)
                {
                    SettingsService.Settings.AutoStart = value;
                    OnPropertyChanged(nameof(IsAutoStart));
                    AutoStartService.Set(value);
                    _trayService?.SetAutoStartChecked(value);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += CommonEvents.Window_PreviewKeyDown;
            AddHotkeyKeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;
            QuickViewHotkeyBox.PreviewKeyDown += CommonEvents.KeyBox_PreviewKeyDown;
            SelectTab(Tab.Link);

            DataContext = this;

            _specialHotkeys =
            [
                new ()
                {
                    Key = Enum.TryParse<Key>(SettingsService.Settings.AddHotkey, out var addHotkey) ? addHotkey : Key.A,
                    Handler = new HandlerWithTag
                    {
                        Handler = OnAddRowHotkeyPressed,
                        Tag = "AddLink"
                    }
                },
                new ()
                {
                    Key = Enum.TryParse<Key>(SettingsService.Settings.CopyHotkey, out var copyHotkey) ? copyHotkey : Key.C,
                    Handler = new HandlerWithTag
                    {
                        Handler = OnCopyClipboardHotkeyPressed,
                        Tag = "CopyClipboard"
                    }
                },
                new()
                {
                    Key = Enum.TryParse<Key>(SettingsService.Settings.QuickViewHotkey, out var quickViewHotkey) ? quickViewHotkey : Key.Q,
                    Handler = new HandlerWithTag
                    {
                        Handler = OnQuickViewHotkeyPressed,
                        Tag = "QuickView"
                    }
                }
            ];

            // Tray icon
            _trayService = new TrayService(IsAutoStart)
            {
                OnAddRow = () => System.Windows.Application.Current.Dispatcher.Invoke(() => ShowAddRowWindow(null, null, RowType.File)),
                OnAutoStartChanged = (val) => System.Windows.Application.Current.Dispatcher.Invoke(() => IsAutoStart = val),
                OnExit = () =>
                {
                    _isExitByTray = true;
                    _trayService.Dispose();
                    System.Windows.Application.Current.Shutdown();
                }
            };

            LoadUIFromSettings();
            RegisterHotkeys();

            if (IsAutoStart)    // 자동 시작 시 트레이만 보이도록 설정
                Hide();
        }

        private void LoadUIFromSettings()
        {
            AddCtrlCheck.IsChecked = SettingsService.Settings.BaseModifier.Contains("Control");
            AddShiftCheck.IsChecked = SettingsService.Settings.BaseModifier.Contains("Shift");
            AddAltCheck.IsChecked = SettingsService.Settings.BaseModifier.Contains("Alt");
            HotkeyService.BaseModifier = ParseModifiers(SettingsService.Settings.BaseModifier);

            AddHotkeyKeyBox.Text = SettingsService.Settings.AddHotkey;
            QuickViewHotkeyBox.Text = SettingsService.Settings.QuickViewHotkey;
            AutoStartCheck.IsChecked = SettingsService.Settings.AutoStart;
        }

        private void LinkTabButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTab(Tab.Link);
        }

        private void ClipboardTabButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTab(Tab.Clipboard);
        }

        private void SelectTab(Tab index)
        {
            if (index == Tab.Link)
            {
                _linkWindow ??= new LinkWindow();
                MainContentControl.Content = _linkWindow;
                HighlightTabButton(LinkTabButton);
            }
            else
            {
                _clipboardWindow ??= new ClipboardWindow();
                MainContentControl.Content = _clipboardWindow;
                HighlightTabButton(ClipboardTabButton);
            }
        }

        private void HighlightTabButton(System.Windows.Controls.Button selected)
        {
            LinkTabButton.Background = selected == LinkTabButton ?
                new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 250)) : System.Windows.Media.Brushes.Transparent;
            ClipboardTabButton.Background = selected == ClipboardTabButton ?
                new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 250)) : System.Windows.Media.Brushes.Transparent;
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

        private async void OnAddRowHotkeyPressed(object? sender, HotkeyEventArgs e)
        {
            SelectTab(Tab.Link);

            string? path = ExplorerBrowserService.GetActiveExplorerPath();
            string? name = string.Empty;
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
                () => ShowAddRowWindow(name, path, type));
            e.Handled = true;
        }

        private void OnCopyClipboardHotkeyPressed(Object? sender, HotkeyEventArgs e)
        {
            SelectTab(Tab.Clipboard);
            _clipboardWindow.CaptureClipboardData();
        }

        private void OnQuickViewHotkeyPressed(object? sender, HotkeyEventArgs e)
        {
            // 이미 객체가 있다면 제거
            if (_quickViewWindow != null)
            {
                _quickViewWindow.Close();
                _quickViewWindow = null;
            }

            _quickViewWindow = new QuickViewWindow(_linkWindow.RowItems, _clipboardWindow.RowItems);
            _quickViewWindow.Closed += (s, args) => _quickViewWindow = null;

            var desktop = SystemParameters.WorkArea;
            _quickViewWindow.Left = desktop.Right - _quickViewWindow.Width - 16;
            _quickViewWindow.Top = desktop.Bottom - _quickViewWindow.Height - 16;
            _quickViewWindow.Show();

            e.Handled = true;
        }

        private void ApplyModifiers_Click(object sender, RoutedEventArgs e)
        {
            var modifier =
                ((AddCtrlCheck.IsChecked == true ? "Control," : string.Empty) +
                (AddShiftCheck.IsChecked == true ? "Shift," : string.Empty) +
                (AddAltCheck.IsChecked == true ? "Alt," : string.Empty)).TrimEnd(',');
            SettingsService.Settings.BaseModifier = modifier;
            HotkeyService.BaseModifier = ParseModifiers(modifier);

            HotkeyService.ResetHotkeys();
            RegisterHotkeys();
            _linkWindow.RegisterHotkeys();
            // _clipboardWindow.RegisterHotkeys();

            System.Windows.MessageBox.Show("Base modifiers has been changed.");
        }

        private void ApplyAddHotkey_Click(object sender, RoutedEventArgs e)
        {
            string hotkey = AddHotkeyKeyBox.Text.Trim();
            _specialHotkeys[0].Key = Enum.TryParse<Key>(hotkey, out var k) ? k : Key.A;
            SettingsService.Settings.AddHotkey = hotkey;

            ChangeSpecialHotkey(_specialHotkeys[0]);
            System.Windows.MessageBox.Show("Add hotkey has been changed.");
        }

        private void QuickViewHotkeyApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string hotkey = QuickViewHotkeyBox.Text.Trim();
            _specialHotkeys[1].Key = Enum.TryParse<Key>(hotkey, out var k) ? k : Key.Q;
            SettingsService.Settings.QuickViewHotkey = hotkey;

            ChangeSpecialHotkey(_specialHotkeys[1]);
            System.Windows.MessageBox.Show("QuickView hotkey has been changed.");
        }


        private static void ChangeSpecialHotkey(SpecialHotkey hotkey)
        {
            HotkeyService.RemoveHotkey(hotkey.Key, hotkey.Handler.Tag);
            HotkeyService.RegisterHotkey(hotkey.Key, hotkey.Handler.Handler, hotkey.Handler.Tag);
        }

        private void RegisterHotkeys()
        {
            foreach (var specialHotkey in _specialHotkeys)
                HotkeyService.RegisterHotkey(specialHotkey.Key, specialHotkey.Handler.Handler, specialHotkey.Handler.Tag);
        }
        private void AutoStartCheck_Checked(object sender, RoutedEventArgs e)
        {
            IsAutoStart = true;
        }

        private void AutoStartCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAutoStart = false;
        }

        private void ShowAddRowWindow(string? name, string? path, RowType type)
        {
            var addRowWindow = new AddRowWindow("Add Link")
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true,
                ShowInTaskbar = true,
                ShowActivated = true
            };
            addRowWindow.SetFields(name, path, null, type);

            if (addRowWindow.ShowDialog() == true)
                _linkWindow.AddRow(addRowWindow.InputName, addRowWindow.InputPath, addRowWindow.InputHotkey, addRowWindow.InputType);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
