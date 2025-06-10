using NHotkey;
using NHotkey.Wpf;
using System.Windows.Input;
using FastLink.Models;
using FastLink.Utils;

namespace FastLink.Services
{
    public static class HotkeyService
    {
        private static readonly Dictionary<string, HotkeyInfo> _hotkeys = [];
        private const string Prefix = "FastLink_";

        public static ModifierKeys BaseModifier = ModifierKeys.Control | ModifierKeys.Shift;

        private static string GetKeyName(Key key)
        {
            return $"{Prefix}{key}";
        }

        public static void RegisterHotkey(Key key, EventHandler<HotkeyEventArgs> handler, object? tag = null)
        {
            if (key == Key.None) return;

            string keyName = GetKeyName(key);

            if (_hotkeys.TryGetValue(keyName, out var info))
            {
                var newHandler = new HandlerWithTag { Handler = handler, Tag = tag };

                if (tag is RowItem row)
                {
                    // 적절한 위치를 찾아서 삽입(이진탐색)
                    int left = 0, right = info.Handlers.Count;
                    while (left < right)
                    {
                        int mid = (left + right) / 2;
                        if (info.Handlers[mid].Tag is RowItem)
                        {
                            RowItem midRow = info.Handlers[mid].Tag as RowItem;
                            Logger.Debug($"{keyName} - {midRow.RowNumber} ({midRow.Name}) vs {row.RowNumber} ({row.Name})");
                        }
                        if (info.Handlers[mid].Tag is not RowItem midTag || midTag.RowNumber < row.RowNumber)
                            left = mid + 1;
                        else right = mid;
                    }
                    info.Handlers.Insert(left, newHandler);
                }
                else info.Handlers.Add(newHandler);
                return;
            }

            // 처음 등록되는 hotkey라면 NHotkey에 등록
            try
            {
                HotkeyManager.Current.Remove(keyName);
                HotkeyManager.Current.AddOrReplace(keyName, key, BaseModifier, (s, e) =>
                {
                    if (_hotkeys.TryGetValue(keyName, out var i))
                        i.Invoke(s, e);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to register key {keyName}. Input keys : {BaseModifier} + {key}");
            }

            var newInfo = new HotkeyInfo
            {
                Key = key
            };
            newInfo.Handlers.Add(new HandlerWithTag { Handler = handler, Tag = tag });
            _hotkeys[keyName] = newInfo;
        }

        public static void RemoveHotkey(Key key, object? tag)
        {
            string keyName = GetKeyName(key);
            if (_hotkeys.TryGetValue(keyName, out var info))
            {
                info.Handlers.RemoveAll(h => Equals(h.Tag, tag));
                if (info.Handlers.Count == 0)
                {
                    HotkeyManager.Current.Remove(keyName);
                    _hotkeys.Remove(keyName);
                }
            }
        }

        public static void ResetHotkeys()
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

        public static void RegisterRowHotkey(RowItem row)
        {
            if (string.IsNullOrWhiteSpace(row.HotkeyKey))
                return;

            string keyStr = row.HotkeyKey.ToUpper();
            if (Enum.TryParse<Key>(keyStr, out var key))
                RegisterHotkey(key, (s, e) => CommonUtils.OpenRowPath(row), row);
        }

        public static void RemoveRowHotkey(RowItem row)
        {
            if (string.IsNullOrWhiteSpace(row.HotkeyKey))
                return;

            if (Enum.TryParse<Key>(row.HotkeyKey.ToUpper(), out var key))
                RemoveHotkey(key, row);
        }

        public static void MoveHandlerToNewHotkey(RowItem row, string oldHotkeyKey)
        {
            if (Enum.TryParse<Key>(oldHotkeyKey, out var key))
                RemoveHotkey(key, row);
            RegisterRowHotkey(row);
        }
    }
}
