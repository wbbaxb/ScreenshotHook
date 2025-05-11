using ScreenshotHook.Presentation.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ScreenshotHook.Presentation.Converters
{
    public class BitToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Bit bitValue)
            {
                switch (bitValue)
                {
                    case Bit.Bit32:
                        return "32位";

                    case Bit.Bit64:
                        return "64位";

                    case Bit.Unknown:
                    default:
                        return "未知";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}