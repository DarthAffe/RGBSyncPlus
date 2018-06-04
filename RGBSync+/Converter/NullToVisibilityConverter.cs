using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RGBSyncPlus.Converter
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value == null) == (string.Equals(parameter?.ToString(), "true", StringComparison.OrdinalIgnoreCase)) ? Visibility.Visible : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
