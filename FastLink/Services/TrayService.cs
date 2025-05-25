using System.Windows;
using FormsNotifyIcon = System.Windows.Forms.NotifyIcon;
using FormsContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using FormsToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;
using FormsToolStripSeparator = System.Windows.Forms.ToolStripSeparator;

namespace FastLink.Services
{
    public class TrayService : IDisposable
    {
        private FormsNotifyIcon _trayIcon;
        private FormsToolStripMenuItem _autoStartMenuItem;

        public Action OnAddRow { get; set; }
        public Action<bool> OnAutoStartChanged { get; set; }
        public Action OnExit { get; set; }

        public TrayService(bool isAutoStart)
        {
            _trayIcon = new FormsNotifyIcon();
            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "thunder.ico");
                _trayIcon.Icon = new(iconPath);
            }
            catch { _trayIcon.Icon = SystemIcons.Application; }
            _trayIcon.Text = "FastLink";
            _trayIcon.Visible = true;

            var contextMenu = new FormsContextMenuStrip();

            var addMenuItem = new FormsToolStripMenuItem("Add Row", null, (s, e) => OnAddRow?.Invoke());

            _autoStartMenuItem = new FormsToolStripMenuItem("Auto Start with Windows", null, (s, e) =>
            {
                _autoStartMenuItem.Checked = !_autoStartMenuItem.Checked;
                OnAutoStartChanged?.Invoke(_autoStartMenuItem.Checked);
            })
            { Checked = isAutoStart };

            var exitMenuItem = new FormsToolStripMenuItem("Exit", null, (s, e) => OnExit?.Invoke());

            contextMenu.Items.Add(addMenuItem);
            contextMenu.Items.Add(_autoStartMenuItem);
            contextMenu.Items.Add(new FormsToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            _trayIcon.ContextMenuStrip = contextMenu;

            _trayIcon.DoubleClick += (s, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = System.Windows.Application.Current.MainWindow;
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                });
            };
        }

        public void SetAutoStartChecked(bool isChecked)
        {
            if (_autoStartMenuItem != null)
                _autoStartMenuItem.Checked = isChecked;
        }

        public void Dispose()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
    }
}
