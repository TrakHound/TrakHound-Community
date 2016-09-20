// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Input;

using TrakHound;
using TrakHound.Plugins;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        private void Main_Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetTabWidths();
        }

        // Keyboard Keys
        private void Main_Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // Always get correct key (ex. Alt)
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Page Tabs
            if (e.Key == Key.Tab && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                ChangePage_Backward();
            }
            else if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ChangePage_Forward();
            }

            // Toggle Developer Console with F12
            //if (key == Key.F12) developerConsole.Shown = !developerConsole.Shown;
        }

        private void Main_Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            messageCenter.Hide();

            //var data = new EventData();
            //data.Id = "WINDOW_CLICKED";
            //Plugin_SendData(data);
        }

    }
}
