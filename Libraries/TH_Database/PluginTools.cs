using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace TH_Database
{

    public static class PluginTools
    {
        public const string PLUGIN_EXTENSION = ".dplugin";

        // Store plugins that are read from CompositionContainer
        class PluginContainer
        {
            [ImportMany(typeof(IDatabasePlugin))]
            public IEnumerable<Lazy<IDatabasePlugin>> Plugins { get; set; }
        }


        public static List<IDatabasePlugin> FindPlugins(string path)
        {
            var result = new List<IDatabasePlugin>();

            var pluginContainer = new PluginContainer();

            CompositionContainer container = null;

            // path is to an individual file
            if (System.IO.File.Exists(path))
            {
                string ext = System.IO.Path.GetExtension(path);

                // Check that the file extension is correct
                if (ext != null)
                {
                    if (ext.ToLower() == PLUGIN_EXTENSION)
                    {
                        var assembly = GetAssemblyFromPath(path);
                        if (assembly != null)
                        {
                            var assemblyCatalog = new AssemblyCatalog(assembly);
                            container = new CompositionContainer(assemblyCatalog);
                        }
                    }
                }
            }
            // path is to a directory
            else if (System.IO.Directory.Exists(path))
            {
                var directoryCatalog = new DirectoryCatalog(path, "*" + PLUGIN_EXTENSION);
                container = new CompositionContainer(directoryCatalog);
            }

            if (container != null)
            {
                // Try Loading the Imports (Plugins)
                try
                {
                    container.SatisfyImportsOnce(pluginContainer);
                }
                catch (System.Reflection.ReflectionTypeLoadException rtex)
                {
                    Console.WriteLine("ReflectionTypeLoadException : " + rtex.Message);

                    foreach (var lex in rtex.LoaderExceptions)
                    {
                        Console.WriteLine("LoaderException : " + lex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception : " + ex.Message);
                }

                if (pluginContainer.Plugins != null)
                {
                    foreach (var lPlugin in pluginContainer.Plugins)
                    {
                        var plugin = lPlugin.Value;
                        result.Add(plugin);
                    }
                }
            }

            return result;
        }

        static Assembly GetAssemblyFromPath(string path)
        {
            Assembly result = null;

            try
            {
                result = Assembly.LoadFile(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(path + " :: " + ex.Message);
            }

            return result;
        }

    }

}
