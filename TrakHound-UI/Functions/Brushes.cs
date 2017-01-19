// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.


using System.Windows;
using System.Windows.Media;

namespace TrakHound_UI.Functions
{
    public static class Brush_Functions
    {

        public static SolidColorBrush GetSolidBrushFromResource(FrameworkElement Parent, string ResourceName)
        {

            SolidColorBrush Result = new SolidColorBrush(Colors.Transparent);

            object var = Parent.TryFindResource(ResourceName);
            if (var != null) Result = (SolidColorBrush)var;

            return Result;

        }

        public static LinearGradientBrush GetLinearGradientBrushFromResource(FrameworkElement Parent, string ResourceName)
        {

            LinearGradientBrush Result = new LinearGradientBrush();

            object var = Parent.TryFindResource(ResourceName);
            if (var != null) Result = (LinearGradientBrush)var;

            return Result;

        }

    }
}
