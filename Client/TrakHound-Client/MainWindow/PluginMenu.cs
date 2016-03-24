// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

using TH_Plugins.Client;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        private void PluginLauncher_BT_Clicked(TH_WPF.Button bt)
        {
            Point point = bt.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
            PluginLauncher.Margin = new Thickness(0, point.Y + bt.RenderSize.Height, 0, 0);

            PluginLauncher.Shown = true;
        }

        private void PluginLauncher_ShownChanged(bool val)
        {
            PluginLauncher_BT.IsSelected = val;
        }

        void AddAppToList(IClientPlugin plugin)
        {
            if (plugin.ShowInAppMenu)
            {
                Menus.Plugins.PluginItem item = new Menus.Plugins.PluginItem();
                item.plugin = plugin;
                item.Text = plugin.Title;
                item.Image = plugin.Image;
                item.Clicked += item_Clicked;

                if (!PluginLauncher.Plugins.Contains(item)) PluginLauncher.Plugins.Add(item);
            }
        }

        void item_Clicked(Menus.Plugins.PluginItem item)
        {
            //if (item.plugin != null) AddPageAsTab(item.plugin, item.plugin.Title, item.plugin.Image);
            if (item.plugin != null) AddTab(item.plugin);
            PluginLauncher.Shown = false;
        }

        void RemoveAppFromList(IClientPlugin plugin)
        {

        }

    }
}
