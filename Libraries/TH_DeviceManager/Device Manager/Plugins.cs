using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;
using System.Data;

using TH_Plugins_Server;
using TH_Global;

namespace TH_DeviceManager
{
    public partial class DeviceManager
    {

        IEnumerable<Lazy<IServerPlugin>> plugins { get; set; }

        public List<IServerPlugin> Plugins { get; set; }

        Plugs PLUGS;

        class Plugs
        {
            [ImportMany(typeof(IServerPlugin))]
            public IEnumerable<Lazy<IServerPlugin>> Plugins { get; set; }
        }

        void LoadPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            Plugins = new List<IServerPlugin>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = TH_Global.FileLocations.Plugins + @"\Server\";
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

        }

        void LoadPlugins(string Path)
        {
            Logger.Log("Searching for Server Plugins in '" + Path + "'");
            if (Directory.Exists(Path))
            {
                try
                {
                    PLUGS = new Plugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(PLUGS);

                    plugins = PLUGS.Plugins;

                    foreach (Lazy<IServerPlugin> lplugin in plugins)
                    {
                        IServerPlugin plugin = lplugin.Value;

                        if (Plugins.ToList().Find(x => x.Name.ToLower() == plugin.Name.ToLower()) == null)
                        {
                            Logger.Log(plugin.Name + " : Plugin Found");
                            Plugins.Add(plugin);
                        }
                        else
                        {
                            Logger.Log(plugin.Name + " : Plugin Already Found");
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadPlugins(directory);
                }
            }
            else Logger.Log("Plugins Directory Doesn't Exist (" + Path + ")");
        }


        List<ConfigurationPage> PluginConfigurationPages;

        void ProcessTablePlugins(DataTable dt)
        {
            if (Plugins != null && dt != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    try
                    {
                        IServerPlugin plugin = lplugin.Value;

                        Type config_type = plugin.Config_Page;

                        object o = Activator.CreateInstance(config_type);

                        ConfigurationPage page = (ConfigurationPage)o;
                    }
                    catch (Exception ex) { Logger.Log("Plugin Exception! : " + ex.Message); }
                }
            }
        }

        void PlugIns_Closing()
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    try
                    {
                        IServerPlugin plugin = lplugin.Value;
                        plugin.Closing();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Plugin Exception! : " + ex.Message);
                    }
                }
            }
        }

    }
}
