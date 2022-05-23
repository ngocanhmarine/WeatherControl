using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TestWeatherControlApp.CacheHelper;
using TestWeatherControlApp.DataModel;
using WeatherControl.DataModel;

namespace TestWeatherControlApp.DataCall
{
    public class APIHelper 
    {
        public static Func<string, ISpecifiedWeatherDataModel> GetWeatherDataMethod
        {
            get { return callOpenWeatherAppMethodApi; }
        }
        public static SpecifiedWeatherDataModel callOpenWeatherAppMethodApi(string city)
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
    }
}
