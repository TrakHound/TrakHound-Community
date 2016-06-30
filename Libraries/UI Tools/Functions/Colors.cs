using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;

namespace UI_Tools.Functions
{
    public static class Colors
    {

        public static Color GetColorFromString(string ColorName)
        {
            Color result = System.Windows.Media.Colors.Transparent;

            object c = null;
            try
            {
                c = ColorConverter.ConvertFromString(ColorName);
            }
            catch (Exception ex) { }
            
            if (c != null) result = (Color)c;

            return result;
        }


        public static Color GetColorFromResource(FrameworkElement Parent, string ResourceName)
        {
            Color result = System.Windows.Media.Colors.Transparent;

            object r = Parent.TryFindResource(ResourceName);
            if (r != null)
            {
                if (r is Color)
                {
                    result = (Color)r;
                }
                else if (r is SolidColorBrush)
                {
                    var brush = r as SolidColorBrush;
                    result = brush.Color;
                }
            }

            return result;
        }

    }
}
