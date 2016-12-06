// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TrakHound_Dashboard.Pages.Dashboard.Footprint.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var control = this.DataContext as Control;

            if (control != null)
            {
                control.Height = control.ActualHeight;
                control.Width = control.ActualWidth;

                double deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:

                        deltaVertical = Math.Min(-e.VerticalChange, control.ActualHeight - control.MinHeight);
                        control.Height -= deltaVertical;
                        break;

                    case VerticalAlignment.Top:

                        deltaVertical = Math.Min(e.VerticalChange, control.ActualHeight - control.MinHeight);
                        Canvas.SetTop(control, Canvas.GetTop(control) + deltaVertical);
                        control.Height -= deltaVertical;
                        break;

                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:

                        deltaHorizontal = Math.Min(e.HorizontalChange, control.ActualWidth - control.MinWidth);
                        Canvas.SetLeft(control, Canvas.GetLeft(control) + deltaHorizontal);
                        control.Width -= deltaHorizontal;
                        break;

                    case HorizontalAlignment.Right:

                        deltaHorizontal = Math.Min(-e.HorizontalChange, control.ActualWidth - control.MinWidth);
                        control.Width -= deltaHorizontal;
                        break;

                    default:
                        break;
                }
            }

            e.Handled = true;
        }
    }
}
