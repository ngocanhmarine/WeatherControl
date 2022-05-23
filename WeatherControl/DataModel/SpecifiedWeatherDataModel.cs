using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WeatherControl.DataModel
{
    public class SpecifiedWeatherDataModel : ISpecifiedWeatherDataModel, INotifyPropertyChanged
    {
        public SpecifiedWeatherDataModel()
        {
            Random rd = new Random();
            this.aqi = rd.Next(1, 500);
        }
        public string city { get; set; }
        public string description { get; set; }
        public float pressure { get; set; }
        public int humidity { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int visibility { get; set; }
        public float wind_speed { get; set; }
        public float wind_deg { get; set; }

        private bool _isCelcius;
        public bool isCelcius {
            get 
            {
                return _isCelcius;
            } 
            set {
                _isCelcius = value;
                RaisePropertyChanged(nameof(isCelcius));
            } 
        }

        private int _aqi;
        public int aqi 
        {
            get {
                return _aqi;
            }
            set { 
                _aqi = value; 
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string propName = "")
        {
            //if (PropertyChanged != null)
            //    PropertyChanged(this, new PropertyChangedEventArgs(propName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
