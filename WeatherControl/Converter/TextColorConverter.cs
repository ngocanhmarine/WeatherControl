using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WeatherControl.Converter
{
    //public class TempConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        float celsius;
    //        float fahrenheit = System.Convert.ToInt32(value);
    //        celsius = (fahrenheit - 32) * 5 / 9;
    //        return fahrenheit;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        float fahrenheit;
    //        float celsius = System.Convert.ToInt32(value);
    //        fahrenheit = (celsius * 9) / 5 + 32;
    //        return fahrenheit;
    //    }
    //}
    public class TextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (((string)value).Length)
            {
                case 3:
                    return "1234";
                case 4:
                    return "123";
                default:
                    return "123";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (((string)value).Length)
            {
                case 3:
                    return "1234";
                case 4:
                    return "123";
                default:
                    return "123";
            }
        }
    }
}
