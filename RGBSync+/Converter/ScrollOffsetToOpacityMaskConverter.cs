using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RGBSyncPlus.Converter
{
    public class ScrollOffsetToOpacityMaskConverter : IMultiValueConverter
    {
        #region Constants

        private static readonly Color TRANSPARENT = Color.FromArgb(0, 0, 0, 0);
        private static readonly Color OPAQUE = Color.FromArgb(255, 0, 0, 0);

        #endregion

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double offset = double.Parse(values[0].ToString());
            double maxHeight = double.Parse(values[1].ToString());
            double height = double.Parse(values[2].ToString());

            double transparencyHeight = double.Parse(parameter.ToString());
            double transparencyFactor = (transparencyHeight - 6) / height;
            double transparencyFadeFactor = (transparencyHeight + 4) / height;

            bool top = !(Math.Abs(offset) < float.Epsilon);
            bool bot = !(Math.Abs(offset - maxHeight) < float.Epsilon);

            if (!top && !bot) return new SolidColorBrush(OPAQUE);

            GradientStopCollection gradientStops = new GradientStopCollection();
            if (top)
            {
                gradientStops.Add(new GradientStop(TRANSPARENT, 0.0));
                gradientStops.Add(new GradientStop(TRANSPARENT, transparencyFactor));
                gradientStops.Add(new GradientStop(OPAQUE, transparencyFadeFactor));
            }
            else
                gradientStops.Add(new GradientStop(OPAQUE, 0.0));

            if (bot)
            {
                gradientStops.Add(new GradientStop(OPAQUE, 1.0 - transparencyFadeFactor));
                gradientStops.Add(new GradientStop(TRANSPARENT, 1.0 - transparencyFactor));
                gradientStops.Add(new GradientStop(TRANSPARENT, 1.0));
            }
            else
                gradientStops.Add(new GradientStop(OPAQUE, 1.0));

            return new LinearGradientBrush(gradientStops, new Point(0.5, 0.0), new Point(0.5, 1.0));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
