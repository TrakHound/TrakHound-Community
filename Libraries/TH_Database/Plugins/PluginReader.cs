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

            Console.WriteLine("Database Plugins --------------------------");
            Logger.Log(plugins.Count.ToString() + " Database Plugins Found");
            Console.WriteLine("------------------------------");
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
            Console.WriteLine("----------------------------------------");
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

        //public IEnumerable<Lazy<IDatabasePlugin>> databasePlugins { get; set; }

        //public static List<IDatabasePlugin> Plugins { get; set; }

        //DatabasePlugs DBPLUGS;

        //class DatabasePlugs
        //{
        //    [ImportMany(typeof(IDatabasePlugin))]
        //    public IEnumerable<Lazy<IDatabasePlugin>> Plugins { get; set; }
        //}

        //public void ReadPlugins()
        //{
        //    string plugin_rootpath = FileLocations.Plugins;

        //    if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

        //    Plugins = new List<IDatabasePlugin>();

        //    string pluginsPath;

        //    // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\Plugins')
        //    pluginsPath = TH_Global.FileLocations.Plugins;
        //    if (Directory.Exists(pluginsPath)) FindPlugins(pluginsPath);

        //    // Load from App root Directory (doesn't overwrite plugins found in System Directory)
        //    //pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
        //    pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
        //    if (Directory.Exists(pluginsPath)) FindPlugins(pluginsPath);


        //    Console.WriteLine("Database Plugins --------------------------");
        //    Console.WriteLine(Plugins.Count.ToString() + " Plugins Found");
        //    Console.WriteLine("------------------------------");
        //    foreach (var plugin in Plugins)
        //    {
        //        IDatabasePlugin plugin = lplugin.Value;

        //        string name = plugin.Name;
        //        string version = null;

        //        // Version Info
        //        Assembly assembly = Assembly.GetAssembly(plugin.GetType());
        //        if (assembly != null)
        //        {
        //            Version v = assembly.GetName().Version;
        //            version = "v" + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + "." + v.Revision.ToString();
        //        }

        //        Console.WriteLine(plugin.Name + " : " + version);
        //    }
        //    Console.WriteLine("----------------------------------------");


        //    Global.Plugins = Plugins;

        //}

        //void FindPlugins(string Path)
        //{
        //    Console.WriteLine("Searching for Database Plugins in : " + Path);

        //    if (Directory.Exists(Path))
        //    {
        //        try
        //        {
        //            DBPLUGS = new DatabasePlugs();

        //            var PageCatalog = new DirectoryCatalog(Path);
        //            var PageContainer = new CompositionContainer(PageCatalog);
        //            PageContainer.SatisfyImportsOnce(DBPLUGS);

        //            databasePlugins = DBPLUGS.PlugIns;

        //            foreach (Lazy<IDatabasePlugin> DBP in databasePlugins.ToList())
        //            {
        //                if (plugins.ToList().Find(x => x.Value.Name.ToLower() == DBP.Value.Name.ToLower()) == null)
        //                {
        //                    plugins.Add(DBP);
        //                }
        //            }
        //        }
        //        catch (System.Reflection.ReflectionTypeLoadException rtex)
        //        {
        //            Console.WriteLine("DatabasePluginReader.GetPlugins() : ReflectionTypeLoadException : " + rtex.Message);

        //            foreach (var lex in rtex.LoaderExceptions)
        //            {
        //                Console.WriteLine("DatabasePluginReader.GetPlugins() : LoaderException : " + lex.Message);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("DatabasePluginReader.GetPlugins() : Exception : " + ex.Message);
        //        }

        //        // Search Subdirectories
        //        foreach (string directory in Directory.GetDirectories(Path))
        //        {
        //            FindPlugins(directory);
        //        }
        //    }
        //}

    }

}
