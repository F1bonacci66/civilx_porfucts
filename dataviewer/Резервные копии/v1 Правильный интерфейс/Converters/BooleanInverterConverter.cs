using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitExporterAddin.Converters
{
    public class BooleanInverterConverter : IValueConverter
    {
        public static readonly BooleanInverterConverter Instance = new BooleanInverterConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }
    }
}

