using System;
using System.Globalization;
using System.Windows.Data;

namespace DuckyProfileSwitcher.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    class BooleanNotConverter : IValueConverter
    {
        private const string ErrorMessage = "This converter only supports booleans and nullable booleans.";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            if (value is bool b)
            {
                return !b;
            }
            throw new InvalidOperationException(ErrorMessage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            if (value is bool b)
            {
                return !b;
            }
            throw new InvalidOperationException(ErrorMessage);
        }
    }
}
