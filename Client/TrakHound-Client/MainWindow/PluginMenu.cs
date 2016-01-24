using System;
using System.Windows;

using TH_Plugins_Client;

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
            if (item.plugin != null) AddPageAsTab(item.plugin, item.plugin.Title, item.plugin.Image);
            PluginLauncher.Shown = false;
        }

        void RemoveAppFromList(IClientPlugin plugin)
        {

        }

    }
}
