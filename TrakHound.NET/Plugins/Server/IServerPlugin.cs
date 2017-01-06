// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel.Composition;

using TrakHound.Configurations;

namespace TrakHound.Plugins.Server
{

    [InheritedExport(typeof(IServerPlugin))]
    public interface IServerPlugin
    {

        /// <summary>
        /// Name of the Plugin
        /// (ex. TH_InstanceTable's name is "TH_InstanceTable")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Called to initialize the plugin using the Configuration file passed as a parameter
        /// </summary>
        /// <param name="Config"></param>
        void Initialize(DeviceConfiguration Config);

        /// <summary>
        /// Called when the plugin is starting
        /// </summary>
        void Starting();

        /// <summary>
        /// Called when the plugin is closing
        /// </summary>
        void Closing();

        /// <summary>
        /// Get Data from sent from another plugin
        /// </summary>
        /// <param name="data"></param>
        void GetSentData(EventData data);

        /// <summary>
        /// Send data to other plugins
        /// </summary>
        event SendData_Handler SendData;

        //Type[] ConfigurationPageTypes { get; }

    }

    public delegate void Status_Handler(string status);

}
