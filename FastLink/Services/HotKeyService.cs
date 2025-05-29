using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using FastLink.Models;
using FastLink.Utils;

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

    public class HotkeyService(ModifierKeys baseModifier = ModifierKeys.Control | ModifierKeys.Shift)
    {
        private readonly Dictionary<string, HotkeyInfo> _hotkeys = new();
        private readonly string _rowPrefix = "RowHotkey_";
        private ModifierKeys _baseModifier = baseModifier;

        public ModifierKeys BaseModifier { get => _baseModifier; set => _baseModifier = value; }

        public void RegisterHotkey(string name, Key key, EventHandler<HotkeyEventArgs> handler, object? tag = null)
        {
            if (key == Key.None) return;

            try
            {
                HotkeyManager.Current.Remove(name);
                HotkeyManager.Current.AddOrReplace(name, key, BaseModifier, handler);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Name : {name}, Input keys : {BaseModifier} + {key}");
            }

            var info = new HotkeyInfo
            {
                Name = name,
                Key = key,
                Modifiers = BaseModifier,
                Handler = handler,
                Tag = tag
            };
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
                        RegisterHotkey(hotkeyName, key, handler, row);
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
