using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WeatherControl.Control2
{
    public class WeatherScrollBar : ScrollBar
    {
        internal int Index { get; set; }
        private bool _ignoreChange = false;
        internal int StartIndex = 0;
        internal int TotalIndex = 0;
        // Max length before full
        internal double widthScrollSum = 0;
        // Max item before full
        internal int countTillScroll = 0;
        // All items width
        internal double widthTotal = 0;
        public WeatherScrollBar()
        {
            this.Orientation = Orientation.Horizontal;
            this.Visibility = Visibility.Visible;
        }
        internal void SetValueInternal(double value)
        {
            if (Value != value)
            {
                bool oldIgnoreFireChanged = _ignoreChange;
                _ignoreChange = true;
                Value = value;
                _ignoreChange = oldIgnoreFireChanged;
            }
        }
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (!_ignoreChange)
            {
                base.OnValueChanged(oldValue, newValue);
                WeatherCommands.Scroll.Execute(new double[] { oldValue, newValue }, this);
            }
        }
    }
}
