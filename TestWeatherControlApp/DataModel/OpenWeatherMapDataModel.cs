using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherControl.DataModel;

namespace TestWeatherControlApp.DataModel
{
    public class OpenWeatherMapDataModel
    {
        public string name { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public int cod { get; set; }
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public Main main;
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public double dt { get; set; }
        public Sys sys { get; set; }

        public class Coord
        {
            public float lon { get; set; }
            public float lat { get; set; }
        }
        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }
        public class Main
        {
            public float temp { get; set; }
            public float pressure { get; set; }
            public int humidity { get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }
        }
        public class Wind
        {
            public float speed { get; set; }
            public int deg { get; set; }
        }

        public class Sys
        {
            public int type { get; set; }
            public int id { get; set; }
            public string country { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
        }
        public ISpecifiedWeatherDataModel toBaseModel()
        {
            SpecifiedWeatherDataModel model = new SpecifiedWeatherDataModel();
            model.city = this.name + " - " + this.sys.country;
            model.description = this.weather[0].description;
            model.pressure = this.main.pressure;
            model.humidity = this.main.humidity;
            model.temp_max = this.main.temp_max;
            model.temp_min = this.main.temp_min;
            model.visibility = this.visibility;
            model.wind_speed = this.wind.speed;
            model.wind_deg = this.wind.deg;
            model.isCelcius = false;

            return model;
        }
    }
}
