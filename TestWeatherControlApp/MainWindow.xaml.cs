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
using TestWeatherControlApp.CacheHelper;
using TestWeatherControlApp.DataModel;
using TestWeatherControlApp.DataCall;
using TestWeatherControlApp.Pages;
using WeatherControl.DataModel;

namespace TestWeatherControlApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            page4_Click(this, null);
        }
        public SpecifiedWeatherDataModel callOpenWeatherAppApi(string city)
        {
            SpecifiedWeatherDataModel dataWeather = new SpecifiedWeatherDataModel();
            dataWeather = (SpecifiedWeatherDataModel)MemoryCacheHelper.GetValue("WeatherCache" + city);
            if (null != dataWeather)
            {
                return dataWeather;
            }

            string APPID = "e7ce6ea31477aa34801c88f733070865";
            var client = new RestClient("https://api.openweathermap.org/data/2.5/weather?q=" + city + "&APPID=" + APPID);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Host", "api.openweathermap.org");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.20.1");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = response.Content;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                OpenWeatherMapDataModel temp = json_serializer.Deserialize(content, typeof(OpenWeatherMapDataModel)) as OpenWeatherMapDataModel;
                dataWeather = temp.toBaseModel() as SpecifiedWeatherDataModel;
                MemoryCacheHelper.Add("WeatherCache" + city, dataWeather, DateTimeOffset.UtcNow.AddHours(1));
            }
            else
            {
                dataWeather = null;
            }
            return dataWeather;
        }

        private void page1_Click(object sender, RoutedEventArgs e)
        {
            Page1_DefaultWeatherControl page1 = new Page1_DefaultWeatherControl();
            spl1.Content = page1;
        }
        private void page2_Click(object sender, RoutedEventArgs e)
        {
            Page2_DynamicLayoutUICustom page2 = new Page2_DynamicLayoutUICustom();
            spl1.Content = page2;
        }

        private void page3_Click(object sender, RoutedEventArgs e)
        {
            Page3_WeatherControl page3 = new Page3_WeatherControl();
            spl1.Content = page3;
        }

        private void page4_Click(object sender, RoutedEventArgs e)
        {
            spl1.Content = null;
            Page4 page = new Page4();
            spl1.Content = page;
        }
    }
}
