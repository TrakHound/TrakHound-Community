// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows.Media;

using TH_Configuration;
using TH_UserManagement.Management;
using TH_Global;

namespace TH_Plugins_Client
{
    /// <summary>
    /// This is the interface for writing Control PlugIns for TrakHound-Client. 
    /// All PlugIns MUST contain the following properties and methods.
    /// </summary>
    [InheritedExport(typeof(IClientPlugin))]
    public interface IClientPlugin : IPage
    {

        #region "Description"

        /// <summary>
        /// Sets the Title of the Plugin
        /// (ex. TH_Dashboard's Title = "Dashboard")
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Used to describe the functions of the Plugin
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Image associated with the Plugin
        /// </summary>
        ImageSource Image { get; }

        /// <summary>
        /// Author's name
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Any other text associated with the author
        /// (ex. copyright statement)
        /// </summary>
        string AuthorText { get; }

        /// <summary>
        /// Image associated with the author
        /// </summary>
        ImageSource AuthorImage { get; }

        /// <summary>
        /// Name of the software license for the plugin
        /// (ex. GPLv3)
        /// </summary>
        string LicenseName { get; }

        /// <summary>
        /// Actual text of the software license for the plugin
        /// </summary>
        string LicenseText { get; }

        #endregion

        #region "Update Information"

        /// <summary>
        /// Url pointing to the 'appinfo' file to use for Automatic Updates
        /// </summary>
        string UpdateFileURL { get; }

        #endregion

        #region "Plugin Properties/Options"

        /// <summary>
        /// Used to set the default parent plugin's name 
        /// (ex. if plugin is desiged for Dashboard, DefaultParent should = "Dashboard")
        /// </summary>
        string DefaultParent { get; }

        /// <summary>
        /// Used to set the default parent plugin's category
        /// (ex. if plugin is designed for Dashboard's Pages category, DefaultParentCategory should = "Pages")
        /// </summary>
        string DefaultParentCategory { get; }

        /// <summary>
        /// Used to turn on/off ability for the plugin to accept "child" plugins
        /// </summary>
        bool AcceptsPlugins { get; }

        /// <summary>
        /// Used to force the plugin to open up when intially loaded/enabled
        /// </summary>
        bool OpenOnStartUp { get; }

        /// <summary>
        /// Used to toggle whether to show in App Launcher menu
        /// </summary>
        bool ShowInAppMenu { get; }

        /// <summary>
        /// Contains the Subcategories for this plugin's "child" plugins
        /// (ex. Dashboard has "Pages" as a subcategory)
        /// </summary>
        List<PluginConfigurationCategory> SubCategories { get; set; }

        /// <summary>
        /// Contains the Plugin's "child" plugins
        /// </summary>
        List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        /// <summary>
        /// Used to initialize the Plugin after the other properties have been set
        /// (ex. called after Devices has been set)
        /// </summary>
        void Initialize();

        void Opened();
        bool Opening();

        void Closed();
        bool Closing();

        #endregion

        #region "Data Events"

        /// <summary>
        /// Get Data from another plugin
        /// </summary>
        /// <param name="de_data"></param>
        void Update_DataEvent(DataEvent_Data de_d);

        /// <summary>
        /// Send data to other plugins
        /// </summary>
        event DataEvent_Handler DataEvent;

        /// <summary>
        /// Send Request to Add/Show this Page in the Client
        /// </summary>
        event PluginTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        /// <summary>
        /// List of Device_Client objects
        /// </summary>
        //List<Device_Client> Devices { get; set; }
        //List<Configuration> Devices { get; set; }
        ObservableCollection<Configuration> Devices { get; set; }

        #endregion

        #region "Options"

        /// <summary>
        /// Sets the OptionsPage object to be displayed in the Clients Options menu as a seperate page
        /// (use if Plugin has parameters or options for how it operates or looks)
        /// </summary>
        IPage Options { get; set; }

        #endregion

    }

    public delegate void DataEvent_Handler(DataEvent_Data de_d);

    public class DataEvent_Data
    {
        public string id { get; set; }

        public object data01 { get; set; }
        public object data02 { get; set; }
        public object data03 { get; set; }
        public object data04 { get; set; }
        public object data05 { get; set; }
    }

    public class PluginCategory
    {
        public string Name { get; set; }
        public List<IClientPlugin> Plugins { get; set; }
    }

    public class PluginShowInfo
    {
        public string PageTitle { get; set; }
        public ImageSource PageImage { get; set; }
        public object Page { get; set; }
    }

    public static class PluginTools
    {
        public const string PLUGIN_EXTENSION = ".cplugin";

        public delegate void SelectedDeviceChanged_Handler(int Index);
        public delegate void ShowRequested_Handler(PluginShowInfo info);


        // Store plugins that are read from CompositionContainer
        class PluginContainer
        {
            [ImportMany(typeof(IClientPlugin))]
            public IEnumerable<Lazy<IClientPlugin>> Plugins { get; set; }
        }


        public static List<IClientPlugin> FindPlugins(string path)
        {
            var result = new List<IClientPlugin>();

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
                        try
                        {
                            var plugin = lPlugin.Value;
                            result.Add(plugin);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Plugin Initialization Error :: " + ex.Message);
                        }
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
