using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using TH_Global;

namespace TH_Database
{

    public static class DatabasePluginReader
    {
        public static void ReadPlugins()
        {
            var plugins = GetPlugins();

            Global.Plugins = plugins;

            Logger.Log("Database Plugins --------------------------");
            Logger.Log(plugins.Count.ToString() + " Database Plugins Found");
            Logger.Log("------------------------------");
            foreach (var plugin in plugins)
            {
                string name = plugin.Name;
                string version = null;

                // Version Info
                Assembly assembly = Assembly.GetAssembly(plugin.GetType());
                if (assembly != null)
                {
                    Version v = assembly.GetName().Version;
                    version = "v" + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + "." + v.Revision.ToString();
                }

                Logger.Log(plugin.Name + " : " + version);
            }
            Logger.Log("----------------------------------------");
        }

        /// <summary>
        /// Get a list of IDatabasePlugins
        /// </summary>
        /// <returns></returns>
        static List<IDatabasePlugin> GetPlugins()
        {
            var result = new List<IDatabasePlugin>();

            string path;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            path = TH_Global.FileLocations.TrakHound + @"\Plugins\";
            if (Directory.Exists(path)) AddPlugins(GetPlugins(path), result);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            path = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(path)) AddPlugins(GetPlugins(path), result);

            return result;
        }

        /// <summary>
        /// Get a list of plugins in the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static List<IDatabasePlugin> GetPlugins(string path)
        {
            var result = new List<IDatabasePlugin>();

            var plugins = PluginTools.FindPlugins(path);
            foreach (var plugin in plugins)
            {
                // Only add if not already in returned list
                if (result.Find(x => x.Name == plugin.Name) == null)
                {
                    result.Add(plugin);
                }
            }

            // Load from subdirectories
            var subdirectories = Directory.GetDirectories(path);
            if (subdirectories != null)
            {
                foreach (var subdirectory in subdirectories)
                {
                    result.AddRange(GetPlugins(subdirectory));
                }
            }

            return result;
        }

        /// <summary>
        /// Add plugins to list making sure that plugins are not repeated in list
        /// </summary>
        /// <param name="newPlugins"></param>
        /// <param name="oldPlugins"></param>
        static void AddPlugins(List<IDatabasePlugin> newPlugins, List<IDatabasePlugin> oldPlugins)
        {
            foreach (var plugin in newPlugins)
            {
                if (oldPlugins.Find(x => x.Name == plugin.Name) == null) oldPlugins.Add(plugin);
            }
        }
        
    }

}
