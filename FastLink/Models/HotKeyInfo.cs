using NHotkey;
using System.Windows.Input;

namespace FastLink.Models
{
    public class HandlerWithTag
    {
        public EventHandler<HotkeyEventArgs> Handler { get; set; }
        public object? Tag { get; set; }
    }

    public class HotkeyInfo
    {
        public Key Key { get; set; }
        public List<HandlerWithTag> Handlers { get; } = new();

        public void Invoke(object sender, HotkeyEventArgs e)
        {
            foreach (var h in Handlers)
                h.Handler?.Invoke(sender, e);
        }
    }

    public class SpecialHotkey
    {
        public Key Key { get; set; }
        public required HandlerWithTag Handler { get; set; }
    }
}