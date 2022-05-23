using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using WeatherControl.DataModel;
using WeatherControl.ElementRenderHelper;

namespace WeatherControl.Control2
{
    [ContentProperty(nameof(Children))]
    public class CustomWeatherControl : System.Windows.Controls.Control, INotifyPropertyChanged
    {
        #region Children
        public UIElementCollection Children
        {
            get { return GetValue(ChildrenProperty) as UIElementCollection; }
            set { this.SetValue(ChildrenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Children.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(UIElementCollection), typeof(CustomWeatherControl), new PropertyMetadata(null));
        protected override int VisualChildrenCount => Children.Count;
        protected override IEnumerator LogicalChildren => Children.GetEnumerator();
        protected override Visual GetVisualChild(int index) => Children[index];

        #endregion
        #region Controls
        private Border bd1 = new Border();
        private System.Windows.Controls.Menu mn1 = new System.Windows.Controls.Menu();
        private Image img1 = new Image();
        private TextBox tb1 = new TextBox();
        private ScrollViewer scv1 = new ScrollViewer();
        private StackPanel splWeather = new StackPanel();
        private StackPanel splButton = new StackPanel();
        private Button btnAdd = new Button();
        private Button btnEdit = new Button();
        private Button btnRemove = new Button();
        private Button btnClear = new Button();
        private TextBlock tbl1 = new TextBlock();
        private TextBlock tbl2 = new TextBlock();
        private bool IsInitialized = false;
        private int count;
        private int itemsMargin = 5;
        private WeatherScrollBar weatherScroll = new WeatherScrollBar();
        #endregion

        public CustomWeatherControl()
        {
            Background = new SolidColorBrush(Colors.Aqua);
            Children = new UIElementCollection(this, this);
            data = new ObservableCollection<SpecifiedWeatherDataModel>();
            Width = 900;
            this.IsInitialized = false;
            CommandBindings.Add(new CommandBinding(WeatherCommands.Scroll, HandleScrollExecuted));
        }

        private void AddControls()
        {
            scv1 = new ScrollViewer()
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            };
            splWeather = new StackPanel() { Height = 230, Orientation = Orientation.Horizontal };
            scv1.Content = splWeather;
            Children.Add(scv1);
        }
        private void addData()
        {
            if (null == this.GetWeatherDataMethod)
            {
                return;
            }
            SpecifiedWeatherDataModel response = GetWeatherDataMethod(tb1.Text) as SpecifiedWeatherDataModel;
            if (null != response && !data.Contains(response))
                data.Add(response);
        }
        public bool addData(string city)
        {
            if (null == this.GetWeatherDataMethod)
                return false;
            SpecifiedWeatherDataModel response = GetWeatherDataMethod(city) as SpecifiedWeatherDataModel;
            if (null != response && !data.Contains(response))
                data.Add(response);
            else return false;
            ShowData();
            return true;
        }
        public void clearData()
        {
            data.Clear();
            ShowData();
        }
        private void ShowData()
        {
            //splWeather.Children.Clear();
            //for (int i = 0; i < data.Count; i++)
            //{
            //    splWeather.Children.Add(ShowSingleData(data[i]));
            //}
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        private Grid ShowSingleData(SpecifiedWeatherDataModel data)
        {
            #region Grid definition

            Grid container = new Grid()
            {
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.LightSlateGray)
            };
            // 3x10 Grid
            container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            for (int i = 0; i < 10; i++)
            {
                container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            }

            #endregion

            #region Grid Elements
            TextBlock city = new TextBlock()
            {
                Text = data.city,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 30
            };
            Grid.SetRow(city, 0);
            Grid.SetColumn(city, 0);
            Grid.SetColumnSpan(city, 3);

            TextBlock lblDescription = new TextBlock() { Text = "Description:" };
            Grid.SetRow(lblDescription, 1);
            Grid.SetColumn(lblDescription, 0);

            TextBlock description = new TextBlock() { Text = data.description };
            Grid.SetRow(description, 1);
            Grid.SetColumn(description, 1);

            TextBlock lblPressure = new TextBlock() { Text = "Pressure:" };
            Grid.SetRow(lblPressure, 2);
            Grid.SetColumn(lblPressure, 0);

            TextBlock pressure = new TextBlock() { Text = data.pressure.ToString() };
            Grid.SetRow(pressure, 2);
            Grid.SetColumn(pressure, 1);

            TextBlock pressureUnit = new TextBlock() { Text = "hPa" };
            Grid.SetRow(pressureUnit, 2);
            Grid.SetColumn(pressureUnit, 2);

            TextBlock lblHumidity = new TextBlock() { Text = "Humidity" };
            Grid.SetRow(lblHumidity, 3);
            Grid.SetColumn(lblHumidity, 0);

            TextBlock humidity = new TextBlock() { Text = data.humidity.ToString() };
            Grid.SetRow(humidity, 3);
            Grid.SetColumn(humidity, 1);

            TextBlock humidityUnit = new TextBlock() { Text = "%" };
            Grid.SetRow(humidityUnit, 3);
            Grid.SetColumn(humidityUnit, 2);

            TextBlock lblTempMax = new TextBlock() { Text = "Temp Max:" };
            Grid.SetRow(lblTempMax, 4);
            Grid.SetColumn(lblTempMax, 0);

            TextBlock tempmax = new TextBlock() { Text = data.temp_max.ToString() };
            Grid.SetRow(tempmax, 4);
            Grid.SetColumn(tempmax, 1);

            TextBlock tempmaxUnit = new TextBlock() { Text = "°F" };
            Grid.SetRow(tempmaxUnit, 4);
            Grid.SetColumn(tempmaxUnit, 2);

            TextBlock lblTempMin = new TextBlock() { Text = "Temp Min:" };
            Grid.SetRow(lblTempMin, 5);
            Grid.SetColumn(lblTempMin, 0);

            TextBlock tempmin = new TextBlock() { Text = data.temp_min.ToString() };
            Grid.SetRow(tempmin, 5);
            Grid.SetColumn(tempmin, 1);

            TextBlock tempminUnit = new TextBlock() { Text = "°F" };
            Grid.SetRow(tempminUnit, 5);
            Grid.SetColumn(tempminUnit, 2);

            TextBlock lblVisibility = new TextBlock() { Text = "Visibility:" };
            Grid.SetRow(lblVisibility, 6);
            Grid.SetColumn(lblVisibility, 0);

            TextBlock visibility = new TextBlock() { Text = data.visibility.ToString() };
            Grid.SetRow(visibility, 6);
            Grid.SetColumn(visibility, 1);

            TextBlock lblWindSpeed = new TextBlock() { Text = "Wind Speed:" };
            Grid.SetRow(lblWindSpeed, 7);
            Grid.SetColumn(lblWindSpeed, 0);

            TextBlock windspeed = new TextBlock() { Text = data.wind_speed.ToString() };
            Grid.SetRow(windspeed, 7);
            Grid.SetColumn(windspeed, 1);

            TextBlock windspeedUnit = new TextBlock() { Text = "km/h" };
            Grid.SetRow(windspeedUnit, 7);
            Grid.SetColumn(windspeedUnit, 2);

            TextBlock lblWindDegree = new TextBlock() { Text = "Wind Degree:" };
            Grid.SetRow(lblWindDegree, 8);
            Grid.SetColumn(lblWindDegree, 0);

            TextBlock winddegree = new TextBlock() { Text = data.wind_speed.ToString() };
            Grid.SetRow(winddegree, 8);
            Grid.SetColumn(winddegree, 1);

            TextBlock winddegreeUnit = new TextBlock() { Text = "°" };
            Grid.SetRow(winddegreeUnit, 8);
            Grid.SetColumn(winddegreeUnit, 2);

            TextBlock lblAqi = new TextBlock() { Text = "AQI:" };
            Grid.SetRow(lblAqi, 9);
            Grid.SetColumn(lblAqi, 0);

            TextBlock aqi = new TextBlock() { Text = data.aqi.ToString() };
            Grid.SetRow(aqi, 9);
            Grid.SetColumn(aqi, 1);

            TextBlock aqiUnit = new TextBlock() { Text = "US AQI" };
            Grid.SetRow(aqiUnit, 9);
            Grid.SetColumn(aqiUnit, 2);

            container.Children.Add(city);
            container.Children.Add(lblDescription);
            container.Children.Add(description);
            container.Children.Add(lblPressure);
            container.Children.Add(pressure);
            container.Children.Add(pressureUnit);
            container.Children.Add(lblHumidity);
            container.Children.Add(humidity);
            container.Children.Add(humidityUnit);
            container.Children.Add(lblTempMax);
            container.Children.Add(tempmax);
            container.Children.Add(tempmaxUnit);
            container.Children.Add(lblTempMin);
            container.Children.Add(tempmin);
            container.Children.Add(tempminUnit);
            container.Children.Add(lblVisibility);
            container.Children.Add(visibility);
            container.Children.Add(lblWindSpeed);
            container.Children.Add(windspeed);
            container.Children.Add(windspeedUnit);
            container.Children.Add(lblWindDegree);
            container.Children.Add(winddegree);
            container.Children.Add(winddegreeUnit);
            container.Children.Add(lblAqi);
            container.Children.Add(aqi);
            container.Children.Add(aqiUnit);
            #endregion

            return container;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            addData("haiphong");
            addData("osaka");
            addData("tokyo");
            addData("aachen");
            addData("lao");
            this.Children.Clear();
            #region ListBox Rendering
            this.Children.Add(weatherScroll);
            for (int i = 0; i < data.Count; i++)
            {
                SpecifiedWeatherDataModel item = data[i];
                WeatherPane weatherPane = new WeatherPane();
                weatherPane.Data = item;
                this.Children.Add(weatherPane);
            }
            #endregion
            for (int i = 1; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                child.Measure(constraint);
            }
            Size size = base.MeasureOverride(constraint);

           
            weatherScroll.Width = this.Width;
            weatherScroll.Height = 20;
            ScrollGetValueFromStartIndex();
            weatherScroll.Measure(new Size(weatherScroll.Width, weatherScroll.Height));
            return size;
        }
        public void ScrollGetValueFromStartIndex()
        {
            bool countSet = false;
            // Max item before full
            weatherScroll.countTillScroll = 0;
            // Max length before full
            weatherScroll.widthScrollSum = 0.0;
            // All items width
            weatherScroll.widthTotal = 0.0;
            for (int i = 1; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                weatherScroll.widthTotal += child.DesiredSize.Width;
                if (!countSet)
                {
                    if (weatherScroll.widthScrollSum + child.DesiredSize.Width > this.Width)
                    {
                        countSet = true;
                    }
                    else
                    {
                        weatherScroll.widthScrollSum += child.DesiredSize.Width;
                        weatherScroll.countTillScroll++;
                    }
                }
            }

            weatherScroll.TotalIndex = this.Children.Count - weatherScroll.countTillScroll;

            // Calculate scroll: get Value from StartIndex ???

            if (weatherScroll.StartIndex == 0 || weatherScroll.Value < weatherScroll.widthScrollSum / weatherScroll.widthTotal)
            {
                weatherScroll.SetValueInternal(this.Width / weatherScroll.widthTotal);
                weatherScroll.StartIndex = 0;
            }
            else
            {
                double widthSum = 0.0;
                for (int i = 1; i < Children.Count - (weatherScroll.TotalIndex - 1 - weatherScroll.StartIndex); i++)
                {
                    UIElement child = Children[i];
                    widthSum += child.DesiredSize.Width;
                }
                weatherScroll.SetValueInternal(widthSum / weatherScroll.widthTotal);
            }
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            double xAxisWidth = 0.0;
            Rect rec1 = new Rect(new Point(0, 20), weatherScroll.DesiredSize);
            weatherScroll.Arrange(rec1);

            for (int i = 1 + weatherScroll.StartIndex; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                xAxisWidth += i == 1 + weatherScroll.StartIndex ? 0 : Children[i - 1].DesiredSize.Width;
                double widthTrim = (xAxisWidth + child.DesiredSize.Width) < this.Width ? child.DesiredSize.Width : this.Width - xAxisWidth;
                if (widthTrim > 0)
                {
                    Rect rec = new Rect(new Point(xAxisWidth, 20), new Size(widthTrim, child.DesiredSize.Height));
                    child.Arrange(rec);
                }
            }
            
            return base.ArrangeOverride(arrangeBounds);
        }

        #region Props
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<SpecifiedWeatherDataModel>), typeof(CustomWeatherControl), new PropertyMetadata(null));

        private ObservableCollection<SpecifiedWeatherDataModel> data
        {
            get { return this.GetValue(DataSourceProperty) as ObservableCollection<SpecifiedWeatherDataModel>; }
            set { this.SetValue(DataSourceProperty, value); }
        }
        public static readonly DependencyProperty GetWeatherDataMethodProperty = DependencyProperty.Register("GetWeatherDataMethod", typeof(Func<string, ISpecifiedWeatherDataModel>), typeof(CustomWeatherControl), new PropertyMetadata(null));

        public Func<string, ISpecifiedWeatherDataModel> GetWeatherDataMethod
        {
            get
            {
                return this.GetValue(GetWeatherDataMethodProperty) as Func<string, ISpecifiedWeatherDataModel>;
            }
            set
            {
                this.SetValue(GetWeatherDataMethodProperty, value);
                RaisePropertyChanged(nameof(GetWeatherDataMethod));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Events
        private void HandleScrollExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            WeatherScrollBar weatherScrollBar = e.OriginalSource as WeatherScrollBar;
            double oldValue = (e.Parameter as double[])[0];
            double newValue = (e.Parameter as double[])[1];
            if (oldValue < newValue)
            {
                switch (oldValue)
                {
                    case 0:
                        weatherScroll.SetValueInternal(this.Width / weatherScroll.widthTotal);
                        weatherScroll.StartIndex = 0;
                        break;

                    default:
                        if (weatherScroll.countTillScroll + weatherScroll.StartIndex < this.Children.Count && weatherScroll.StartIndex < weatherScroll.TotalIndex)
                        {
                            UIElement nextChild = this.Children[weatherScroll.countTillScroll + weatherScroll.StartIndex + 1];
                            weatherScroll.SetValueInternal(oldValue + (nextChild != null ? nextChild.DesiredSize.Width : 0) / weatherScroll.widthTotal);
                            weatherScroll.StartIndex++;
                        }
                        break;
                }
            }
            else
            {
                if (weatherScroll.StartIndex == 0)
                {
                    weatherScroll.SetValueInternal(this.Width / weatherScroll.widthTotal);
                }
                else
                {
                    UIElement prevChild = this.Children[weatherScroll.countTillScroll + weatherScroll.StartIndex];
                    weatherScroll.SetValueInternal(oldValue - prevChild.DesiredSize.Width / weatherScroll.widthTotal);
                    weatherScroll.StartIndex--;
                }
            }
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }
        #endregion
    }
}
