using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherControl.DataModel
{

    public interface ISpecifiedWeatherDataModel
    {
         string city { get; set; }
         string description { get; set; }
         float pressure { get; set; }
         int humidity { get; set; }
         float temp_min { get; set; }
         float temp_max { get; set; }
         int visibility { get; set; }
         float wind_speed { get; set; }
         float wind_deg { get; set; }
        bool isCelcius { get; set; }
        
        //Air quality index
        int aqi { get; set; }
    }
}
