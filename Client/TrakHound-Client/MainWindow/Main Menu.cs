// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

using TH_WPF;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        private void MainMenu_BT_Clicked(Button bt)
        {
            Point point = bt.TransformToAncestor(root).Transform(new Point(0, 0));
            MainMenu.Margin = new Thickness(0, point.Y + bt.RenderSize.Height, 5, 0);

            MainMenu.Shown = true;
        }

        private void MainMenu_ShownChanged(bool val)
        {
            MainMenu_BT.IsSelected = val;
        }

    }
}
