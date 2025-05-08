using System;
using System.Globalization;
using System.Windows.Data;

namespace ScreenshotHook.Presentation.Converters
{
    public class ByteRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (int.TryParse(str, out int result))
                {
                    if (result < 0)
                    {
                        result = 0;
                    }

                    if (result > 255)
                    {
                        result = 255;
                    }
                }
                else
                {
                    result = 0;
                }

                return result;
            }

            return 0;
        }
    }
}