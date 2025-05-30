﻿using System.ComponentModel;

namespace FastLink.Models
{
    public enum RowType { File, Web }

    public class RowItem : INotifyPropertyChanged
    {
        private int _rowNumber;
        private string _name = "";
        private string _path = "";
        private RowType _type = RowType.File;
        private string _hotkeyKey = "";

        public int RowNumber
        {
            get => _rowNumber;
            set
            {
                if (_rowNumber != value)
                {
                    _rowNumber = value;
                    OnPropertyChanged(nameof(RowNumber));
                }
            }
        }
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } }
        }
        public string Path
        {
            get => _path;
            set { if (_path != value) { _path = value; OnPropertyChanged(nameof(Path)); } }
        }
        public RowType Type
        {
            get => _type;
            set { if (_type != value) { _type = value; OnPropertyChanged(nameof(Type)); } }
        }
        public string HotkeyKey
        {
            get => _hotkeyKey;
            set { if (_hotkeyKey != value) { _hotkeyKey = value; OnPropertyChanged(nameof(HotkeyKey)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
