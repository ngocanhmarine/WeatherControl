using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WeatherControl.WeatherControl2Element
{
    class WeatherControl2StackPanel : StackPanel
    {
        public WeatherControl2StackPanel()
        {
            ControlTemplate ctmp = new ControlTemplate(typeof(Control));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            ctmp.VisualTree = border;
            border.SetValue(Border.BorderBrushProperty,new TemplateBindingExtension(Control.BorderBrushProperty));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            DockPanel dockPanel = new DockPanel() { LastChildFill = false, Width = 1000 };
        }
    }
}
