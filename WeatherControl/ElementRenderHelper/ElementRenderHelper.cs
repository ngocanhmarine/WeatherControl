using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeatherControl.Control2;

namespace WeatherControl.ElementRenderHelper
{
    public static class ElementRenderHelper
    {
        private static Brush blackBrush = new SolidColorBrush(Colors.Black);
        private static Pen blackPen = new Pen(new SolidColorBrush(Colors.Black), 1.5);
        public static bool ElementRender(this CustomWeatherControl directParent, DrawingContext drawingContext, Visual visual, Point position)
        {
            if (visual is TextBlock)
            {
                directParent.TextBlockRender(drawingContext, visual as TextBlock, position);
            }
            else if (visual is Border)
            {
                directParent.BorderRender(drawingContext, visual as Border, position);
            }

            return true;
        }
        public static bool TextBlockRender(this CustomWeatherControl directParent, DrawingContext drawingContext, TextBlock tb, Point position)
        {
            Typeface typeface = new Typeface(tb.FontFamily,
            tb.FontStyle,
            tb.FontWeight,
            tb.FontStretch);
            FormattedText formattedText = new FormattedText(
                tb.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                tb.FlowDirection,
                typeface,
                tb.FontSize,
                tb.Foreground);

            double width = tb.ActualWidth;
            double height = tb.ActualHeight;

            var parent = VisualTreeHelper.GetParent(tb);
            if (null == parent) parent = directParent;
            while (!(parent is Control))
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (null == parent) parent = directParent;
            }

            drawingContext.DrawText(formattedText, position);
            return true;
        }
        public static bool BorderRender(this CustomWeatherControl directParent, DrawingContext drawingContext, Border bd, Point position)
        {
            drawingContext.DrawRoundedRectangle(bd.Background, new Pen(bd.BorderBrush, 1), new Rect(position, new Point(bd.ActualWidth, bd.ActualHeight)), bd.CornerRadius.TopLeft, bd.CornerRadius.BottomRight);
            if (null != bd.Child)
            {
                directParent.ElementRender(drawingContext, bd.Child, position);
            }
            return true;
        }
        //public static bool TextBoxRender(this CustomWeatherControl directParent, DrawingContext drawingContext, TextBox tb ,Point position)
        //{
        //    Size size = tb.DesiredSize;

        //}
        
    }
}
