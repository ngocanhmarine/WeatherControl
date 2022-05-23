using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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
    public class TempUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tempUnit = string.Empty;
            if ((bool)value)
            {
                return "°C";
            }
            else
            {
                return "°F";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tempUnit = string.Empty;
            if ((bool)value)
            {
                return "°F";
            }
            else
            {
                return "°C";
            }
        }
    }
}
