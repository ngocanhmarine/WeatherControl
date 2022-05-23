using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WeatherControl.Converter
{
    public class AqiColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int aqi = (int)value;
            SolidColorBrush color;
            if (aqi < 50) color = Brushes.Green;
            else if (aqi < 100) color = Brushes.Yellow;
            else if (aqi < 150) color = Brushes.Orange;
            else if (aqi < 200) color = Brushes.Red;
            else if (aqi < 300) color = Brushes.Purple;
            else color = Brushes.Maroon;
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int aqi = (int)value;
            SolidColorBrush color;
            if (aqi < 50) color = Brushes.Green;
            else if (aqi < 100) color = Brushes.Yellow;
            else if (aqi < 150) color = Brushes.Orange;
            else if (aqi < 200) color = Brushes.Red;
            else if (aqi < 300) color = Brushes.Purple;
            else color = Brushes.Maroon;
            return color;
        }
    }
    public class AqiImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int aqi = (int)value;
            BitmapImage img;
            //if (aqi < 50) img = new BitmapImage(new Uri("pack://application:,,,/img/AQI/green.png", UriKind.RelativeOrAbsolute));
            //else if (aqi < 100) img = new BitmapImage(new Uri("pack://application:,,,/img/AQI/yellow.png", UriKind.RelativeOrAbsolute));
            //else if (aqi < 150) img = new BitmapImage(new Uri("pack://application:,,,/img/AQI/orange.png", UriKind.RelativeOrAbsolute));
            //else if (aqi < 200) img = new BitmapImage(new Uri("pack://application:,,,/img/AQI/red.png", UriKind.RelativeOrAbsolute));
            //else if (aqi < 300) img = new BitmapImage(new Uri("pack://application:,,,/img/AQI/purple.png", UriKind.RelativeOrAbsolute));
            //else img = new BitmapImage(new Uri("pack://application:,,,/img/AQI/maroon.png", UriKind.RelativeOrAbsolute));

            if (aqi < 50) img = new BitmapImage(new Uri("/WeatherControl;component/img/aqi/green.png", UriKind.RelativeOrAbsolute));
            else if (aqi < 100) img = new BitmapImage(new Uri("/WeatherControl;component/img/aqi/yellow.png", UriKind.RelativeOrAbsolute));
            else if (aqi < 150) img = new BitmapImage(new Uri("/WeatherControl;component/img/aqi/orange.png", UriKind.RelativeOrAbsolute));
            else if (aqi < 200) img = new BitmapImage(new Uri("/WeatherControl;component/img/aqi/red.png", UriKind.RelativeOrAbsolute));
            else if (aqi < 300) img = new BitmapImage(new Uri("/WeatherControl;component/img/aqi/purple.png", UriKind.RelativeOrAbsolute));
            else img = new BitmapImage(new Uri("/WeatherControl;component/img/aqi/maroon.png", UriKind.RelativeOrAbsolute));
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
