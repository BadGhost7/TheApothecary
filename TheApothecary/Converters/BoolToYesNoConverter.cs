using System;
using System.Globalization;
using System.Windows.Data;

namespace TheApothecary.Converters
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool boolValue && boolValue) ? "Да" : "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string str && str == "Да";
        }
    }
}