using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeatherControl.DataModel;
using WeatherControl.CacheHelper;
using System.Collections.ObjectModel;
using System.Threading;

namespace WeatherControl
{
    /// <summary>
    /// Interaction logic for WeatherControl.xaml
    /// </summary>
    public partial class WeatherUserControl : UserControl
    {
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<SpecifiedWeatherDataModel>), typeof(WeatherUserControl));

        private ObservableCollection<SpecifiedWeatherDataModel> data
        {
            get { return this.GetValue(DataSourceProperty) as ObservableCollection<SpecifiedWeatherDataModel>; }
            set { this.SetValue(DataSourceProperty, value); }
        }
        #region Public Property
        public static DependencyProperty GetWeatherDataMethodProperty = DependencyProperty.Register("GetWeatherDataMethod", typeof(Func<string, ISpecifiedWeatherDataModel>), typeof(WeatherUserControl));
        public Func<string, ISpecifiedWeatherDataModel> GetWeatherDataMethod
        {
            get
            {
                return this.GetValue(GetWeatherDataMethodProperty) as Func<string, ISpecifiedWeatherDataModel>;
            }
            set
            {
                this.SetValue(GetWeatherDataMethodProperty, value);
            }
        }

        public static DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(WeatherUserControl));
        public BitmapImage Icon
        {
            get
            {
                return this.GetValue(IconProperty) as BitmapImage;
            }
            set
            {
                iconImg.Source = value;
                this.SetValue(IconProperty, value);
            }
        }

        #endregion

        public WeatherUserControl(Func<string, ISpecifiedWeatherDataModel> GetWeatherDataMethod)
        {
            this.GetWeatherDataMethod = GetWeatherDataMethod;
            InitializeComponent();
            ApplyLanguage("en-US");
            this.Icon = new BitmapImage(new Uri("img/icon.jpg", UriKind.Relative));
            data = new ObservableCollection<SpecifiedWeatherDataModel>();
            txtCities.Text = "Hanoi";
            addData();
        }

        public void addData()
        {
            SpecifiedWeatherDataModel response = GetWeatherDataMethod(txtCities.Text) as SpecifiedWeatherDataModel;
            if (null != response && !data.Contains(response))
            {
                data.Add(response);
            }
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            addData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            data.Remove(lvWeather.SelectedItem as SpecifiedWeatherDataModel);
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            data.Clear();
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            SpecifiedWeatherDataModel selectedItem = lvWeather.SelectedItem as SpecifiedWeatherDataModel;
            int selectedIndex = data.IndexOf(selectedItem);
            if (selectedIndex == -1)
            {
                return;
            }
            SpecifiedWeatherDataModel response = GetWeatherDataMethod(txtCities.Text) as SpecifiedWeatherDataModel;
            if (!(null != response && new SpecifiedWeatherDataModel() != response))
            {
                return;
            }
            int targetIndex = data.IndexOf(response);
            if (targetIndex > -1)
            {
                SpecifiedWeatherDataModel temp = data[selectedIndex];
                data[selectedIndex] = data[targetIndex];
                data[targetIndex] = temp;
            }
            else
            {
                data[targetIndex] = response;
            }
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void changeTempUnit(object sender, MouseButtonEventArgs e)
        {
            SpecifiedWeatherDataModel clickedItem = ((sender as FrameworkElement).DataContext as SpecifiedWeatherDataModel);
            data[lvWeather.Items.IndexOf(clickedItem)].isCelcius = !data[lvWeather.Items.IndexOf(clickedItem)].isCelcius;
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void TempConverter_MouseEnter(object sender, MouseEventArgs e)
        {
            SpecifiedWeatherDataModel model = (sender as TextBlock).DataContext as SpecifiedWeatherDataModel;
            float temp_max = model.temp_max;
            float temp_min = model.temp_min;
            float new_temp_max;
            float new_temp_min;

            model.isCelcius = !model.isCelcius;
            if (model.isCelcius)
            {
                new_temp_max = (temp_max - 32) * 5 / 9;
                new_temp_min = (temp_min - 32) * 5 / 9;
            }
            else
            {
                new_temp_max = temp_max * 9 / 5 + 32;
                new_temp_min = temp_min * 9 / 5 + 32;
            }
            data[lvWeather.Items.IndexOf(model)].temp_min = new_temp_min;
            data[lvWeather.Items.IndexOf(model)].temp_max = new_temp_max;
            data[lvWeather.Items.IndexOf(model)].isCelcius = model.isCelcius;
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;

            //TextBlock txtTempMaxUnit = ((sender as TextBlock).Parent as FrameworkElement).FindName("tempMaxUnit") as TextBlock;
            //TextBlock txtTempMinUnit = ((sender as TextBlock).Parent as FrameworkElement).FindName("tempMinUnit") as TextBlock;
            //if (model.isCelcius)
            //{

            //    txtTempMaxUnit.Text = "°C";
            //    txtTempMinUnit.Text = "°C";
            //}
            //else
            //{
            //    txtTempMaxUnit.Text = "°F";
            //    txtTempMinUnit.Text = "°F";
            //}
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ApplyLanguage(menuItem.Tag.ToString());
        }
        private void ApplyLanguage(string cultureName = null)
        {
            if (cultureName != null)
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            }
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "vi-VN":
                    dict.Source = new Uri("/WeatherControl;component/ResourcesDictionary/Vietnamese.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("/WeatherControl;component/ResourcesDictionary/English.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
            // check/uncheck the language menu items based on the current culture
            foreach (var item in languageChoice.Items)
            {
                MenuItem menuItem = item as MenuItem;
                if (menuItem.Tag.ToString() == Thread.CurrentThread.CurrentCulture.Name)
                {
                    menuItem.IsChecked = true;
                }
                else
                {
                    menuItem.IsChecked = false;
                }
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

        }
    }
}
