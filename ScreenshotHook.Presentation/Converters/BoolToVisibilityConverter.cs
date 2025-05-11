using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScreenshotHook.Presentation.Converters
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val)
            {
                if (!Inverse)
                {
                    return val ? Visibility.Visible : Visibility.Collapsed;
                }

                return val ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}