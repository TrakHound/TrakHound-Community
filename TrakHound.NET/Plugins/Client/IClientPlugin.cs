// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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
        /// Used to describe the functions of the Plugin
        /// </summary>
        string Description { get; }

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
        /// Used to force the plugin to open up when intially loaded/enabled
        /// </summary>
        bool OpenOnStartUp { get; }

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
        /// Sets the OptionsPage object to be displayed in the Clients Options menu as a seperate page
        /// (use if Plugin has parameters or options for how it operates or looks)
        /// </summary>
        IPage Options { get; set; }

    }    

}
