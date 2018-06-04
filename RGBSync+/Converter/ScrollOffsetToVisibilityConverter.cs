using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RGBSyncPlus.Converter
{
    // Based on: http://stackoverflow.com/a/28679767
    public class ScrollOffsetToVisibilityConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool top = "top".Equals(parameter?.ToString(), StringComparison.OrdinalIgnoreCase);

            double offset = double.Parse(values[0].ToString());
            double maxHeight = double.Parse(values[1].ToString());

            return (top && Math.Abs(offset) < float.Epsilon) || (!top && Math.Abs(offset - maxHeight) < float.Epsilon)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
