using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherControl.DataModel
{
    public interface IGetWeatherDataMethod
    {
        Func<string, ISpecifiedWeatherDataModel> GetWeatherDataMethod { get; }
    }
}
