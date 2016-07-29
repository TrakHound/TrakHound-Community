// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

using TrakHound.Plugins.Client;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        //private void PluginLauncher_BT_Clicked(TrakHound_UI.Button bt)
        //{
        //    Point point = bt.TransformToAncestor(root).Transform(new Point(0, 0));
        //    PluginLauncher.Margin = new Thickness(0, point.Y + bt.RenderSize.Height, 0, 0);

        //    PluginLauncher.Shown = true;
        //}

        //private void PluginLauncher_ShownChanged(bool val)
        //{
        //    //PluginLauncher_BT.IsSelected = val;
        //}

        //void AddAppToList(IClientPlugin plugin)
        //{
        //    if (plugin.ShowInAppMenu)
        //    {
        //        var item = new Menus.MenuItem();
        //        item.Text = plugin.Title;
        //        item.Image = plugin.Image;
        //        item.DataObject = plugin;
        //        item.Clicked += Item_Clicked;

        //        var bt = new TrakHound_UI.Button();
        //        bt.ButtonContent = item;

        //        if (!PluginLauncher.Plugins.Contains(bt)) PluginLauncher.Plugins.Add(bt);
        //    }
        //}

        //private void Item_Clicked(object obj)
        //{
        //    if (obj != null) AddTab((IClientPlugin)obj);
        //    PluginLauncher.Shown = false;
        //}

        //void RemoveAppFromList(IClientPlugin plugin)
        //{

        //}

    }
}
