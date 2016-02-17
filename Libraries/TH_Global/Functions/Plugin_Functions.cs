using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;

namespace TH_Global.Functions
{
    public static class Plugin_Functions
    {

        //public static Type PluginType { get; set;}

        //// Store plugins that are read from CompositionContainer
        //class PluginContainer
        //{
        //    [ImportMany]
        //    public IEnumerable<Lazy<object>> Plugins { get; set; }
        //}

        //public static List<object> FindPlugins(string path)
        //{
        //    var result = new List<object>();

        //    var pluginContainer = new PluginContainer();

        //    var PageCatalog = new DirectoryCatalog(path);
        //    var PageContainer = new CompositionContainer(PageCatalog);
        //    PageContainer.SatisfyImportsOnce(pluginContainer);

        //    if (pluginContainer.Plugins != null)
        //    {
        //        foreach (var lPlugin in pluginContainer.Plugins)
        //        {
        //            var plugin = lPlugin.Value;
        //            Console.WriteLine(plugin.GetType().ToString()); 
        //        }
        //    }

        //    return result;
        //}






    }
}
