using System.Globalization;
using System.Windows.Data;

namespace FastLink.Utils
{
    public class EditSaveIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEditing = value is bool b && b;
            return isEditing ? "ContentSave" : "Pencil";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
