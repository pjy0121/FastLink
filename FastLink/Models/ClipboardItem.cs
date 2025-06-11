namespace FastLink.Models
{
    public class ClipboardItem<T> : RowItem
    {
        private T _data;

        public T Data
        {
            get => _data;
            set
            {
                if (!Equals(_data, value))
                {
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }
        }
    }
}
