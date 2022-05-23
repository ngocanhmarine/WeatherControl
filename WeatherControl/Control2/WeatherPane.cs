using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WeatherControl.DataModel;

namespace WeatherControl.Control2
{
    public class WeatherPane : Panel
    {
        #region My Elements
        private Border bd = new Border() { Background = Brushes.DarkGoldenrod, Margin = new Thickness(5) };
        #endregion
        public SpecifiedWeatherDataModel Data
        {
            get { return (SpecifiedWeatherDataModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DataProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("DataProperty", typeof(SpecifiedWeatherDataModel), typeof(WeatherPane), new PropertyMetadata(null));


        public WeatherPane()
        {

        }

        private void setChild()
        {
            #region add Child
            if (null == Data)
            {
                return;
            }
            SpecifiedWeatherDataModel item = Data;
            TextBlock city = new TextBlock()
            {
                Text = item.city,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 30
            };

            TextBlock lblDescription = new TextBlock() { Text = "Description:" };
            TextBlock description = new TextBlock() { Text = item.description };
            TextBlock descriptionUnit = new TextBlock();
            TextBlock lblPressure = new TextBlock() { Text = "Pressure:" };
            TextBlock pressure = new TextBlock() { Text = item.pressure.ToString() };
            TextBlock pressureUnit = new TextBlock() { Text = "hPa" };
            TextBlock lblHumidity = new TextBlock() { Text = "Humidity" };
            TextBlock humidity = new TextBlock() { Text = item.humidity.ToString() };
            TextBlock humidityUnit = new TextBlock() { Text = "%" };
            TextBlock lblTempMax = new TextBlock() { Text = "Temp Max:" };
            TextBlock tempmax = new TextBlock() { Text = item.temp_max.ToString() };
            TextBlock tempmaxUnit = new TextBlock() { Text = "°F" };
            TextBlock lblTempMin = new TextBlock() { Text = "Temp Min:" };
            TextBlock tempmin = new TextBlock() { Text = item.temp_min.ToString() };
            TextBlock tempminUnit = new TextBlock() { Text = "°F" };
            TextBlock lblVisibility = new TextBlock() { Text = "Visibility:" };
            TextBlock visibility = new TextBlock() { Text = item.visibility.ToString() };
            TextBlock visibilityUnit = new TextBlock();
            TextBlock lblWindSpeed = new TextBlock() { Text = "Wind Speed:" };
            TextBlock windspeed = new TextBlock() { Text = item.wind_speed.ToString() };
            TextBlock windspeedUnit = new TextBlock() { Text = "km/h" };
            TextBlock lblWindDegree = new TextBlock() { Text = "Wind Degree:" };
            TextBlock winddegree = new TextBlock() { Text = "↑" };
            //RotateTransform rotateTransform = new RotateTransform(item.wind_deg);
            //winddegree.RenderTransform = rotateTransform;
            TextBlock winddegreeUnit = new TextBlock() { Text = item.wind_deg.ToString() + "°" };
            TextBlock lblAqi = new TextBlock() { Text = "AQI:" };
            TextBlock aqi = new TextBlock() { Text = item.aqi.ToString() };
            TextBlock aqiUnit = new TextBlock() { Text = "US AQI" };
            #endregion
            this.Children.Add(bd);
            this.Children.Add(city);
            this.Children.Add(lblDescription);
            this.Children.Add(description);
            this.Children.Add(descriptionUnit);
            this.Children.Add(lblPressure);
            this.Children.Add(pressure);
            this.Children.Add(pressureUnit);
            this.Children.Add(lblHumidity);
            this.Children.Add(humidity);
            this.Children.Add(humidityUnit);
            this.Children.Add(lblTempMax);
            this.Children.Add(tempmax);
            this.Children.Add(tempmaxUnit);
            this.Children.Add(lblTempMin);
            this.Children.Add(tempmin);
            this.Children.Add(tempminUnit);
            this.Children.Add(lblVisibility);
            this.Children.Add(visibility);
            this.Children.Add(visibilityUnit);
            this.Children.Add(lblWindSpeed);
            this.Children.Add(windspeed);
            this.Children.Add(windspeedUnit);
            this.Children.Add(lblWindDegree);
            this.Children.Add(winddegree);
            this.Children.Add(winddegreeUnit);
            this.Children.Add(lblAqi);
            this.Children.Add(aqi);
            this.Children.Add(aqiUnit);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            setChild();
            double maxWidth = 0;
            double maxHeight = 0;
            double bdMargin = bd.Margin.Left + bd.Margin.Right;
            for (int i = 1; i < Children.Count; i++)
            {
                // City
                if (i < 2)
                {
                    Children[i].Measure(new Size(availableSize.Width + bdMargin, availableSize.Height));
                    maxWidth = Math.Max(maxWidth, double.IsInfinity(Children[i].DesiredSize.Width) ? 0 : Children[i].DesiredSize.Width + bdMargin);
                    maxHeight += double.IsInfinity(Children[i].DesiredSize.Height) ? 0 : Children[i].DesiredSize.Height;
                }
                // Label
                else if (i % 3 == 2)
                {
                    Children[i].Measure(new Size(availableSize.Width + bd.Margin.Left, availableSize.Height));
                }
                // Data
                else if (i % 3 == 0)
                {

                    if ((Children[i] as TextBlock).Text == "↑")
                    {
                        Children[i].Measure(new Size(availableSize.Width, availableSize.Height));
                        RotateTransform rotateTransform = new RotateTransform(Data.wind_deg, Children[i].DesiredSize.Width / 2, Children[i].DesiredSize.Height / 2);
                        Children[i].RenderTransform = rotateTransform;
                    }
                    else
                    {
                        Children[i].Measure(new Size(availableSize.Width, availableSize.Height));
                    }
                }
                // Unit
                else
                {
                    double w1 = double.IsInfinity(Children[i - 2].DesiredSize.Width) ? 0 : Children[i - 2].DesiredSize.Width;
                    double w2 = double.IsInfinity(Children[i - 1].DesiredSize.Width) ? 0 : Children[i - 1].DesiredSize.Width;
                    double w3 = double.IsInfinity(Children[i].DesiredSize.Width) ? 0 : Children[i].DesiredSize.Width;
                    double h1 = double.IsInfinity(Children[i - 2].DesiredSize.Height) ? 0 : Children[i - 2].DesiredSize.Height;
                    double h2 = double.IsInfinity(Children[i - 1].DesiredSize.Height) ? 0 : Children[i - 1].DesiredSize.Height;
                    double h3 = double.IsInfinity(Children[i].DesiredSize.Height) ? 0 : Children[i].DesiredSize.Height;
                    Children[i].Measure(new Size(availableSize.Width + bd.Margin.Right, availableSize.Height));
                    maxWidth = Math.Max(maxWidth, w1 + w2 + w3 + bdMargin);
                    maxHeight += Math.Max(Math.Max(h1, h2), h3);
                }
            }
            Border border = Children[0] as Border;
            border.Width = maxWidth + bdMargin;
            border.Height = maxHeight + bd.Margin.Top + bd.Margin.Bottom;
            border.Measure(new Size(border.Width, border.Height));
            return new Size(border.Width, border.Height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double yAxisHeight = 0.0;
            double xAxisWidth = bd.Margin.Left + bd.Margin.Right;
            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                if (i == 0 && child is Border)
                {
                    Rect rec = new Rect(new Point(0, 0), new Size(child.DesiredSize.Width, child.DesiredSize.Height));
                    child.Arrange(rec);
                }
                else if (i == 1)
                {
                    Rect rec = new Rect(new Point(bd.Margin.Left, yAxisHeight), new Size(child.DesiredSize.Width + bd.Margin.Right, child.DesiredSize.Height));
                    child.Arrange(rec);
                    yAxisHeight += child.DesiredSize.Height;
                }
                else if (i % 3 == 2)
                {

                    Rect rec = new Rect(new Point(bd.Margin.Left, yAxisHeight), child.DesiredSize);
                    child.Arrange(rec);
                }
                else if (i % 3 == 0)
                {
                    if ((Children[i] as TextBlock).Text == "↑")
                    {
                        double w1 = double.IsInfinity(Children[i - 1].DesiredSize.Width) ? 0 : Children[i - 1].DesiredSize.Width;
                        Rect rec = new Rect(new Point(bd.Margin.Left + w1, yAxisHeight), new Size(child.DesiredSize.Width, child.DesiredSize.Height));
                        child.Arrange(rec);
                    }
                    else
                    {
                        double w1 = double.IsInfinity(Children[i - 1].DesiredSize.Width) ? 0 : Children[i - 1].DesiredSize.Width;
                        Rect rec = new Rect(new Point(bd.Margin.Left + w1, yAxisHeight), new Size(child.DesiredSize.Width, child.DesiredSize.Height));
                        child.Arrange(rec);
                    }
                }
                else
                {
                    double w1 = double.IsInfinity(Children[i - 2].DesiredSize.Width) ? 0 : Children[i - 2].DesiredSize.Width;
                    double w2 = double.IsInfinity(Children[i - 1].DesiredSize.Width) ? 0 : Children[i - 1].DesiredSize.Width;
                    double h1 = double.IsInfinity(Children[i - 2].DesiredSize.Height) ? 0 : Children[i - 2].DesiredSize.Height;
                    double h2 = double.IsInfinity(Children[i - 1].DesiredSize.Height) ? 0 : Children[i - 1].DesiredSize.Height;
                    double h3 = double.IsInfinity(Children[i].DesiredSize.Height) ? 0 : Children[i].DesiredSize.Height;
                    Rect rec = new Rect(new Point(bd.Margin.Left + w1 + w2, yAxisHeight), new Size(child.DesiredSize.Width + bd.Margin.Right, child.DesiredSize.Height));
                    child.Arrange(rec);
                    yAxisHeight += Math.Max(Math.Max(h1, h2), h3);
                }
            }
            return base.ArrangeOverride(finalSize);
        }
    }
}
