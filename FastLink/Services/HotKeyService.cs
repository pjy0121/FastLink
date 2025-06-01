using NHotkey;
using NHotkey.Wpf;
using System.Windows.Input;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink.Services
{
    public class HotkeyService(ModifierKeys baseModifier = ModifierKeys.Control | ModifierKeys.Shift)
    {
        private readonly Dictionary<string, HotkeyInfo> _hotkeys = new();
        private const string Prefix = "FastLink_";
        private ModifierKeys _baseModifier = baseModifier;

        public ModifierKeys BaseModifier { get => _baseModifier; set => _baseModifier = value; }

        private static string GetKeyName(Key key)
        {
            return $"{Prefix}{key}";
        }

        public void RegisterHotkey(Key key, EventHandler<HotkeyEventArgs> handler, object? tag = null)
        {
            if (key == Key.None) return;

            // 이미 등록된 hotkey가 있으면 handler만 추가
            string keyName = GetKeyName(key);
            if (_hotkeys.TryGetValue(keyName, out var info))
            {
                info.Handlers.Add(new HandlerWithTag
                {
                    Handler = handler,
                    Tag = tag
                });
                return;
            }

            // 처음 등록되는 hotkey라면 NHotkey에 등록
            try
            {
                HotkeyManager.Current.Remove(keyName);
                HotkeyManager.Current.AddOrReplace(keyName, key, BaseModifier, (s, e) =>
                {
                    if (_hotkeys.TryGetValue(keyName, out var info))
                        info.Invoke(s, e);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to register key {keyName}. Input keys : {BaseModifier} + {key}");
            }

            var newInfo = new HotkeyInfo
            {
                Key = key,
                Handlers =
                {
                    new HandlerWithTag
                    {
                        Handler = handler,
                        Tag = tag
                    }
                }
            };
            _hotkeys[keyName] = newInfo;
        }

        public void ResetHotkeys()
        {
            foreach (var keyName in _hotkeys.Keys.ToList())
            {
                try
                {
                    HotkeyManager.Current.Remove(keyName);
                }
                catch { }
            }
            _hotkeys.Clear();
        }

        public void RegisterRowHotkey(RowItem row)
        {
            if (!string.IsNullOrWhiteSpace(row.HotkeyKey))
            {
                string keyStr = row.HotkeyKey.ToUpper();
                if (Enum.TryParse<Key>(keyStr, out var key))
                    RegisterHotkey(key, (s, e) => CommonUtils.OpenRowPath(row), row);
            }
        }

        public void RemoveRowHotkey(RowItem row)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.HotkeyKey))
                return;

            string keyName = GetKeyName(Enum.Parse<Key>(row.HotkeyKey.ToUpper()));
            if (_hotkeys.TryGetValue(keyName, out var info))
            {
                info.Handlers.RemoveAll(h => Equals(h.Tag, row));
                if (info.Handlers.Count == 0)
                {
                    HotkeyManager.Current.Remove(keyName);
                    _hotkeys.Remove(keyName);
                }
            }
        }
    }
}
