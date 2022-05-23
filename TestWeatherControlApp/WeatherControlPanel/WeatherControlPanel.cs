using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TestWeatherControlApp.DataCall;

namespace TestWeatherControlApp.WeatherControlPanel
{
    public class WeatherControlPanel : StackPanel
    {
        public WeatherControlPanel()
        {
            WeatherControl.WeatherControl wc = new WeatherControl.WeatherControl();
            wc.GetWeatherDataMethod = APIHelper.GetWeatherDataMethod;
            ResourceDictionary newDictionary = new ResourceDictionary();
            newDictionary.Source = new Uri("Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
            if (this.Resources.MergedDictionaries.Count == 0)
                this.Resources.MergedDictionaries.Add(newDictionary);
            else
                this.Resources.MergedDictionaries[0] = newDictionary;
            BitmapImage bmp = new BitmapImage(new Uri("/TestWeatherControlApp;component/img/CustomIcon.png", UriKind.RelativeOrAbsolute));
            wc.Icon = bmp;
            this.Children.Add(wc);
        }
        //protected override Size MeasureOverride(Size constraint)
        //{
        //    return base.MeasureOverride(constraint);
        //    Double childHeight = 0.0;
        //    Double childWidth = 0.0;
        //    Size size = new Size(0, 0);
        //    foreach (UIElement child in InternalChildren)
        //    {
        //        child.Measure(new Size(constraint.Width, constraint.Height));
        //        if (child.DesiredSize.Width > childWidth)
        //        {
        //            childWidth = child.DesiredSize.Width;
        //        }
        //        childHeight += child.DesiredSize.Height;
        //    }
        //    size.Width = double.IsPositiveInfinity(constraint.Width) ? childWidth : constraint.Width;
        //    size.Height = double.IsPositiveInfinity(constraint.Height) ? childHeight : constraint.Height;
        //    return size;
        //}
        //protected override Size ArrangeOverride(Size arrangeSize)
        //{
        //    double yAxisHeight = 0.0;
        //    foreach (UIElement child in InternalChildren)
        //    {
        //        Rect rec = new Rect(new Point(0, yAxisHeight), child.DesiredSize);
        //        child.Arrange(rec);
        //        yAxisHeight += child.DesiredSize.Height;
        //    }
        //    return arrangeSize;
        //}
    }
}
