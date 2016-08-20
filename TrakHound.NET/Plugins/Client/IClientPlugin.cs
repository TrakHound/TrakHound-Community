// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Media;

using TrakHound;
using TrakHound.Configurations;

namespace TrakHound.Plugins.Client
{
    /// <summary>
    /// This is the interface for writing Plugins for TrakHound-Client. 
    /// All Plugins MUST contain the following properties and methods.
    /// </summary>
    [InheritedExport(typeof(IClientPlugin))]
    public interface IClientPlugin : IPage
    {

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

        ///// <summary>
        ///// Author's name
        ///// </summary>
        //string Author { get; }

        ///// <summary>
        ///// Author's copyright statement
        ///// </summary>
        //string Copyright { get; }

        ///// <summary>
        ///// Image associated with the author
        ///// </summary>
        //ImageSource AuthorImage { get; }

        ///// <summary>
        ///// Name of the software license for the plugin
        ///// (ex. GPLv3)
        ///// </summary>
        //string LicenseName { get; }

        ///// <summary>
        ///// Actual text of the software license for the plugin
        ///// </summary>
        //string LicenseText { get; }




        /// <summary>
        /// Url pointing to the 'appinfo' file to use for Automatic Updates (if applicable)
        /// </summary>
        //string UpdateFileURL { get; }




        /// <summary>
        /// Used to set the default parent plugin's name 
        /// (ex. if plugin is desiged for Dashboard, ParentPlugin should = "Dashboard")
        /// </summary>
        string ParentPlugin { get; }

        /// <summary>
        /// Used to set the default parent plugin's category
        /// (ex. if plugin is designed for Dashboard's Pages category, ParentPluginCategory should = "Pages")
        /// </summary>
        string ParentPluginCategory { get; }

        /// <summary>
        /// Used to turn on/off ability for the plugin to accept "child" plugins
        /// </summary>
        //bool AcceptsPlugins { get; }

        /// <summary>
        /// Used to force the plugin to open up when intially loaded/enabled
        /// </summary>
        bool OpenOnStartUp { get; }

        /// <summary>
        /// Used to toggle whether to show in App Launcher menu
        /// </summary>
        //bool ShowInAppMenu { get; }

        /// <summary>
        /// Contains the Subcategories for this plugin's "child" plugins
        /// (ex. Dashboard has "Pages" as a subcategory)
        /// </summary>
        List<PluginConfigurationCategory> SubCategories { get; set; }

        /// <summary>
        /// Contains the Plugin's "child" plugins
        /// </summary>
        List<IClientPlugin> Plugins { get; set; }



        /// <summary>
        /// Used to initialize the Plugin after the other properties have been set
        /// (ex. called after Devices has been set)
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the plugin is shown
        /// </summary>
        void Opened();

        /// <summary>
        /// Called when the plugin is going to be shown. Return false to cancel
        /// </summary>
        /// <returns></returns>
        bool Opening();

        /// <summary>
        /// Called when the plugin is closed
        /// </summary>
        void Closed();

        /// <summary>
        /// Called when the plugin is going to be closed. Return false to cancel
        /// </summary>
        /// <returns></returns>
        bool Closing();


        /// <summary>
        /// Get Data from another plugin
        /// </summary>
        /// <param name="data"></param>
        void GetSentData(EventData data);

        /// <summary>
        /// Send data to other plugins
        /// </summary>
        event SendData_Handler SendData;

        /// <summary>
        /// List of Device_Client objects
        /// </summary>
        //ObservableCollection<DeviceConfiguration> Devices { get; set; }

        /// <summary>
        /// Sets the OptionsPage object to be displayed in the Clients Options menu as a seperate page
        /// (use if Plugin has parameters or options for how it operates or looks)
        /// </summary>
        IPage Options { get; set; }

    }    

}
