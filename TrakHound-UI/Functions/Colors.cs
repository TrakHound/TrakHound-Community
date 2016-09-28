// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using System.Windows;
using System.Windows.Media;

namespace TrakHound_UI.Functions
{
    public static class Color_Functions
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
