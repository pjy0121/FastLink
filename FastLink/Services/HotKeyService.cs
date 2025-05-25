using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using FastLink.Models;

namespace FastLink.Services
{
    public class HotkeyInfo
    {
        public string Name { get; set; }
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public object? Tag { get; set; } // RowItem 등 추가 데이터
        public EventHandler<HotkeyEventArgs> Handler { get; set; }
    }

    public class HotkeyService
    {
        private readonly Dictionary<string, HotkeyInfo> _hotkeys = new();
        private readonly string _rowPrefix = "RowHotkey_";

        public void RegisterHotkey(string name, Key key, ModifierKeys modifiers, EventHandler<HotkeyEventArgs> handler, object? tag = null)
        {
            try { HotkeyManager.Current.Remove(name); } catch { }
            var info = new HotkeyInfo
            {
                Name = name,
                Key = key,
                Modifiers = modifiers,
                Handler = handler,
                Tag = tag
            };
            HotkeyManager.Current.AddOrReplace(name, key, modifiers, handler);
            _hotkeys[name] = info;
        }

        public void UnregisterHotkey(string name)
        {
            try { HotkeyManager.Current.Remove(name); } catch { }
            _hotkeys.Remove(name);
        }

        public void RegisterRowHotkeys(IEnumerable<RowItem> rows, EventHandler<HotkeyEventArgs> handler)
        {
            foreach (var key in new List<string>(_hotkeys.Keys))
            {
                if (key.StartsWith(_rowPrefix))
                    UnregisterHotkey(key);
            }

            foreach (var row in rows)
            {
                if (!string.IsNullOrWhiteSpace(row.HotkeyKey))
                {
                    string keyStr = row.HotkeyKey.ToUpper();
                    if (Enum.TryParse<Key>(keyStr, out var key))
                    {
                        string hotkeyName = _rowPrefix + keyStr;
                        RegisterHotkey(hotkeyName, key, ModifierKeys.Control | ModifierKeys.Shift, handler, row);
                    }
                }
            }
        }

        public HotkeyInfo? GetHotkeyInfo(string name)
        {
            _hotkeys.TryGetValue(name, out var info);
            return info;
        }

        public RowItem? GetRowByKey(string key)
        {
            string name = _rowPrefix + key.ToUpper();
            if (_hotkeys.TryGetValue(name, out var info) && info.Tag is RowItem row)
                return row;
            return null;
        }
    }
}
