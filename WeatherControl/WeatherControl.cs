using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace WeatherControl
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WeatherControl"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WeatherControl;assembly=WeatherControl"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:WeatherControl/>
    ///
    /// </summary>
    public class WeatherControl : Control, IGetWeatherDataMethod, INotifyPropertyChanged
    {
        #region Children Collection
        public List<UIElement> Children
        {
            get { return (List<UIElement>)GetValue(ChildrenProperty); }
        }

        // Using a DependencyProperty as the backing store for Children.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey ChildrenPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Children), typeof(List<UIElement>), typeof(WeatherControl), new FrameworkPropertyMetadata(new List<UIElement>()));
        public static readonly DependencyProperty ChildrenProperty = ChildrenPropertyKey.DependencyProperty;
        #endregion
        public WeatherControl()
        {
            IconUriKind = UriKind.RelativeOrAbsolute;
            ThemesUriKind = UriKind.RelativeOrAbsolute;
        }
        protected override Size MeasureOverride(Size constraint)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                child.Measure(constraint);
            }
            return base.MeasureOverride(constraint);
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            double yAxisHeight = 0.0;
            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                yAxisHeight += i == 0 ? 0 : Children[i - 1].DesiredSize.Height;
                Rect rec = new Rect(new Point(0, yAxisHeight), arrangeBounds);
                child.Arrange(rec);
            }
            return base.ArrangeOverride(arrangeBounds);
        }

        public override void EndInit()
        {
            base.EndInit();
            if (null != ThemesUri)
            {
                ResourceDictionary newDictionary = new ResourceDictionary();
                try
                {
                    newDictionary.Source = new Uri(ThemesUri, ThemesUriKind);

                    if (this.Resources.MergedDictionaries.Count == 0)
                        this.Resources.MergedDictionaries.Add(newDictionary);
                    else
                        this.Resources.MergedDictionaries[0] = newDictionary;
                    this.ApplyTemplate();
                }
                catch (UriFormatException e)
                {

                }
            }
        }
        
        static WeatherControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WeatherControl), new FrameworkPropertyMetadata(typeof(WeatherControl)));
           
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
            MenuItem languageChoice = this.Template.FindName("languageChoice", this) as MenuItem;
            if (null != languageChoice)
            {
                foreach (var item in languageChoice.Items)
                {
                    MenuItem menuItem = item as MenuItem;
                    menuItem.IsChecked = menuItem.Tag.ToString() == Thread.CurrentThread.CurrentCulture.Name ? true : false;
                }
            }
        }
        public override void OnApplyTemplate()
        {
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
            
            base.OnApplyTemplate();
            MenuItem menuExit = this.Template.FindName("menuExit", this) as MenuItem;
            if (null != menuExit) menuExit.Click += menuExit_Click;

            MenuItem English = this.Template.FindName("English", this) as MenuItem;
            if (null != English) English.Click += Language_Click;
            MenuItem Vietnamese = this.Template.FindName("Vietnamese", this) as MenuItem;
            if (null != Vietnamese) Vietnamese.Click += Language_Click;

            Button btnAdd = this.Template.FindName("btnAdd", this) as Button;
            if (null != btnAdd) btnAdd.Click += Add_Click;
            Button btnEdit = this.Template.FindName("btnEdit", this) as Button;
            if (null != btnEdit) btnEdit.Click += btnEdit_Click;
            Button btnDelete = this.Template.FindName("btnDelete", this) as Button;
            if (null != btnDelete) btnDelete.Click += btnDelete_Click;
            Button btnClear = this.Template.FindName("btnClear", this) as Button;
            if (null != btnClear) btnClear.Click += btnClear_Click;
            
            ApplyLanguage("en-US");
            if (this.Icon == new BitmapImage() || this.Icon == null)
            {
                if (null != this.IconUri)
                {
                    try
                    {
                        this.Icon = new BitmapImage(new Uri(this.IconUri, this.IconUriKind));
                    }
                    catch (UriFormatException e)
                    {

                    }
                }
                else
                {
                    this.Icon = new BitmapImage(new Uri("pack://application:,,,/img/icon.jpg", UriKind.RelativeOrAbsolute));
                    //this.Icon = new BitmapImage(new Uri("pack://application:,,,/img/aqi/purple.png", UriKind.RelativeOrAbsolute));
                }
            }
            Image iconImg = this.Template.FindName("iconImg", this) as Image;
            if (null != iconImg) iconImg.Source = this.Icon;

            data = new ObservableCollection<SpecifiedWeatherDataModel>();
            txtCities = "Hanoi";
            if (null != this.GetWeatherDataMethod)
            {
                addData();
            }
            lvWeather.DataContextChanged += lsWeatherChanged;
            
        }
        public void addData()
        {
            if (null == this.GetWeatherDataMethod)
            {
                return;
            }
            SpecifiedWeatherDataModel response = GetWeatherDataMethod(txtCities) as SpecifiedWeatherDataModel;
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
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
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
            data.Remove(lvWeather.SelectedItem as SpecifiedWeatherDataModel);
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
            data.Clear();
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
            SpecifiedWeatherDataModel selectedItem = lvWeather.SelectedItem as SpecifiedWeatherDataModel;
            int selectedIndex = data.IndexOf(selectedItem);
            if (selectedIndex == -1)
            {
                return;
            }
            SpecifiedWeatherDataModel response = GetWeatherDataMethod(txtCities) as SpecifiedWeatherDataModel;
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
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
            SpecifiedWeatherDataModel clickedItem = ((sender as FrameworkElement).DataContext as SpecifiedWeatherDataModel);
            data[lvWeather.Items.IndexOf(clickedItem)].isCelcius = !data[lvWeather.Items.IndexOf(clickedItem)].isCelcius;
            lvWeather.ItemsSource = null;
            lvWeather.ItemsSource = data;
        }

        private void TempConverter_MouseEnter(object sender, MouseEventArgs e)
        {
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;
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
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void lsWeatherChanged( object sender,DependencyPropertyChangedEventArgs e)
        {
            ListView lvWeather = this.Template.FindName("lvWeather", this) as ListView;

            foreach (ListViewItem item in lvWeather.Items)
            {
                TextBlock tMax = item.FindName("tempMax") as TextBlock;
                TextBlock tMin = item.FindName("tempMin") as TextBlock;
                tMax.MouseUp += TempConverter_MouseEnter;
                tMin.MouseUp += TempConverter_MouseEnter;
            } 
        }

        #region Public Property
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<SpecifiedWeatherDataModel>), typeof(WeatherControl), new PropertyMetadata(null));

        private ObservableCollection<SpecifiedWeatherDataModel> data
        {
            get { return this.GetValue(DataSourceProperty) as ObservableCollection<SpecifiedWeatherDataModel>; }
            set { this.SetValue(DataSourceProperty, value); }
        }
        public static readonly DependencyProperty GetWeatherDataMethodProperty = DependencyProperty.Register("GetWeatherDataMethod", typeof(Func<string, ISpecifiedWeatherDataModel>), typeof(WeatherControl), new PropertyMetadata(null));

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

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(WeatherControl), new PropertyMetadata(null));
        public BitmapImage Icon
        {
            get
            {
                return this.GetValue(IconProperty) as BitmapImage;
            }
            set
            {
                this.SetValue(IconProperty, value);
                RaisePropertyChanged(nameof(Icon));
            }
        }

        public static readonly DependencyProperty ThemesUriProperty = DependencyProperty.Register("ThemesUri", typeof(string), typeof(WeatherControl), new PropertyMetadata(null));
        public string ThemesUri
        {
            get { return this.GetValue(ThemesUriProperty) as string; }
            set
            {
                this.SetValue(ThemesUriProperty, value);
                if (null != ThemesUri)
                {
                    ResourceDictionary newDictionary = new ResourceDictionary();
                    try
                    {
                        newDictionary.Source = new Uri(ThemesUri, ThemesUriKind);

                        if (this.Resources.MergedDictionaries.Count == 0)
                            this.Resources.MergedDictionaries.Add(newDictionary);
                        else
                            this.Resources.MergedDictionaries[0] = newDictionary;
                        this.ApplyTemplate();
                    }
                    catch (UriFormatException e)
                    {

                    }
                }
                RaisePropertyChanged(nameof(ThemesUri));
            }
        }
        private UriKind _themesUriKind;
        public UriKind ThemesUriKind
        {
            get { return _themesUriKind; }
            set
            {
                _themesUriKind = value;
                if (null != ThemesUri)
                {
                    ResourceDictionary newDictionary = new ResourceDictionary();
                    try
                    {
                        newDictionary.Source = new Uri(ThemesUri, ThemesUriKind);
                        if (this.Resources.MergedDictionaries.Count == 0)
                            this.Resources.MergedDictionaries.Add(newDictionary);
                        else
                            this.Resources.MergedDictionaries[0] = newDictionary;
                        this.ApplyTemplate();
                    }
                    catch (UriFormatException e)
                    {

                    }
                }
                RaisePropertyChanged(nameof(ThemesUriKind));
            }
        }

        public static readonly DependencyProperty IconUriProperty = DependencyProperty.Register("IconUri", typeof(string), typeof(WeatherControl), new PropertyMetadata(null));
        public string IconUri
        {
            get
            {
                return this.GetValue(IconUriProperty) as string;
            }
            set
            {
                this.SetValue(IconUriProperty, value);
                try
                {
                    this.Icon = new BitmapImage(new Uri(this.IconUri, this.IconUriKind));
                    Image iconImg = this.Template.FindName("iconImg", this) as Image;
                    if (null != iconImg) iconImg.Source = this.Icon;
                }
                catch (Exception e)
                {

                }
                RaisePropertyChanged(nameof(IconUri));
            }
        }
        private UriKind _iconUriKind;
        public UriKind IconUriKind
        {
            get
            {
                return this._iconUriKind;
            }
            set
            {
                _iconUriKind = value;
                if (null != this.IconUri)
                {
                    try
                    {
                        this.Icon = new BitmapImage(new Uri(this.IconUri, this.IconUriKind));
                    }
                    catch (UriFormatException e)
                    {

                    }
                }
                RaisePropertyChanged(nameof(IconUriKind));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public string txtCities
        {
            get
            {
                return (this.Template.FindName("txtCities", this) as TextBox).Text;
            }
            set
            {
                (this.Template.FindName("txtCities", this) as TextBox).Text = value;
            }
        }

        #endregion
    }
}
