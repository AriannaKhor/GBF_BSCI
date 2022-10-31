using System;
using System.Globalization;
using System.Windows.Data;

namespace UIModule.DataMarkers.Helpers
{
	[ValueConversion(typeof(double), typeof(double))]
    public class SlotSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2;
        }
    }
}
