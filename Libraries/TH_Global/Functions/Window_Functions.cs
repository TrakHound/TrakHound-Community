using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_Global.Functions
{
    public static class Window_Functions
    {

        public static void CenterWindow(System.Windows.Window window)
        {
            double screen_width = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screen_height = System.Windows.SystemParameters.PrimaryScreenHeight;

            double window_width = window.ActualWidth;
            double window_height = window.ActualHeight;

            double center_x = (screen_width / 2) - (window_width / 2);
            double center_y = (screen_height / 2) - (window_height / 2);

            window.Left = center_x;
            window.Top = center_y;
        }

    }
}
