using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

public class AppSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _linkFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FastLink", "fastlink_rows.json");
    public string LinkFilePath
    {
        get => _linkFilePath;
        set => SetField(ref _linkFilePath, value);
    }

    private string _clipboardFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FastLink", "fastlink_clipboards.json");
    public string ClipboardFilePath
    {
        get => _clipboardFilePath;
        set => SetField(ref _clipboardFilePath, value);
    }

    private string _baseModifier = "Control,Shift";
    public string BaseModifier
    {
        get => _baseModifier;
        set => SetField(ref _baseModifier, value);
    }

    private string _addHotkey = "A";
    public string AddHotkey
    {
        get => _addHotkey;
        set => SetField(ref _addHotkey, value);
    }

    private string _quickViewHotkey = "Q";
    public string QuickViewHotkey
    {
        get => _quickViewHotkey;
        set => SetField(ref _quickViewHotkey, value);
    }

    private bool _autoStart = false;
    public bool AutoStart
    {
        get => _autoStart;
        set => SetField(ref _autoStart, value);
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
